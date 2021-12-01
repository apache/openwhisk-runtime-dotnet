/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using Apache.Openwhisk.Runtime.Minimal;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

/// <summary>
/// using an injected service to make things more decoupled.
/// We're going to rely on the framework to deserialize the init body into an object.
/// </summary>
internal class RuntimeService
{
    private readonly SemaphoreSlim _initSemaphoreSlim = new SemaphoreSlim(1, 1);

    public bool Initialized { get; private set; }
    private Type Type { get; set; }
    private MethodInfo Method { get; set; }


    private ConstructorInfo Constructor { get; set; }
    private bool AwaitableMethod { get; set; }
    private Run FunctionToRun { get; set; }

    public RuntimeService()
    {
        Initialized = false;
    }

    internal static IResult LogErrorToConsoleAndReturnErrorJson(string errorMessage)
    {
        Console.Error.WriteLine(errorMessage);
        return Results.Json(new { error = errorMessage }, statusCode: 500);
    }

    /// <summary>
    /// Now that we have initialized and created the function we can run it, log results/errors and return. https://github.com/apache/openwhisk/blob/master/docs/actions-new.md#activation
    /// </summary>
    /// <param name="runbody"></param>
    /// <returns></returns>
    internal async Task<IResult> RunRequest(RunPostBody? runbody)
    {
        Console.WriteLine("Attempting to run request with:" + JsonSerializer.Serialize(runbody));

        if (Initialized == false || FunctionToRun.Type == null || FunctionToRun.Method == null || FunctionToRun.Constructor == null)
        {
            return LogErrorToConsoleAndReturnErrorJson("Cannot invoke an uninitalized action.");
        }

        try
        {
            //the value property of the run post body is a json string of values that will become parameters for the function
            JsonObject? valueObject = runbody?.value != null ? JsonNode.Parse(runbody.value)?.AsObject() : null;

            if (runbody != null)
            {
                var runPostBodyProps = typeof(RunPostBody).GetProperties();
                foreach (var prop in runPostBodyProps.Where(x => !x.Name.Equals("value")))
                {
                    try
                    {
                        string? envValue = prop.GetValue(runbody) as string;
                        string envKey = $"__OW_{prop.Name.ToUpperInvariant()}";
                        Environment.SetEnvironmentVariable(prop.Name, envValue);
                    }
                    catch (Exception)
                    {
                        return LogErrorToConsoleAndReturnErrorJson($"Unable to set environment variable for the \" {prop.Name}\" token");
                    }

                }
            }

            object owObject = FunctionToRun.Constructor.Invoke(new object[] { });
            try
            {
                JsonObject? output;
                if (AwaitableMethod)
                {
                    output = (JsonObject)await (dynamic)FunctionToRun.Method.Invoke(owObject, new object[] { valueObject });
                }
                else
                {
                    output = (JsonObject)FunctionToRun.Method.Invoke(owObject, new object[] { valueObject });
                }

                if (output == null)
                {
                    return LogErrorToConsoleAndReturnErrorJson("The action returned null");
                }

                return Results.Ok(new { Message = "Hello World!", Runbody = JsonSerializer.Serialize(runbody) });
            }
            catch (Exception ex)
            {
                return LogErrorToConsoleAndReturnErrorJson(ex.Message
#if DEBUG
                                                          + ", " + ex.StackTrace
#endif
                    );
            }

        }
        finally
        {
            WriteLogMarkers();
        }
    }

    /// <summary>
    /// Essentially we need to check and see if we can create a function to invoke and run from the binary uploaded file. https://github.com/apache/openwhisk/blob/master/docs/actions-new.md#initialization
    /// </summary>
    /// <param name="initBody">Json body that contains values to initalize the container</param>
    /// <returns></returns>
    internal async Task<IResult> Initialize(InitPostBody? initBody)
    {
        Console.WriteLine("Attempting to initialize request with:" + JsonSerializer.Serialize(initBody));
        await _initSemaphoreSlim.WaitAsync();
        try
        {

            if (Initialized)
            {
                return LogErrorToConsoleAndReturnErrorJson("Cannot initialize the action more than once.");
            }

            if (initBody?.value == null || initBody.value.main == null || initBody.value.code == null)
            {
                return LogErrorToConsoleAndReturnErrorJson("Missing main/no code to execute.");
            }

            if (!initBody.value.binary)
            {
                return LogErrorToConsoleAndReturnErrorJson("code must be binary (zip file).");
            }

            string[] mainParts = initBody.value.main.Split("::");
            if (mainParts.Length != 3)
            {
                return LogErrorToConsoleAndReturnErrorJson("main required format is \"Assembly::Type::Function\".");
            }

            string tempPath = Path.Combine(Environment.CurrentDirectory, Guid.NewGuid().ToString());
            string tempFile = Path.GetTempFileName();
            await File.WriteAllBytesAsync(tempFile, Convert.FromBase64String(initBody.value.code));
            try
            {
                System.IO.Compression.ZipFile.ExtractToDirectory(tempFile, tempPath);
            }
            catch (Exception)
            {
                return LogErrorToConsoleAndReturnErrorJson("Unable to decompress package");
            }
            finally
            {
                File.Delete(tempFile);
            }

            Environment.CurrentDirectory = tempPath;
            string assemblyFile = $"{mainParts[0]}.dll";
            string assemblyPath = Path.Combine(tempPath, assemblyFile);

            if (!File.Exists(assemblyPath))
            {
                return LogErrorToConsoleAndReturnErrorJson($"Unable to locate requested assembly (\"{assemblyFile}\").");
            }

            try
            {
                Env? owEnvironmentVars = initBody?.value?.env;
                if (owEnvironmentVars != null)
                {
                    var envValues = typeof(Env).GetProperties();
                    foreach (var prop in envValues)
                    {
                        if (initBody?.value?.env != null)
                        {
                            string? envValue = prop.GetValue(owEnvironmentVars) as string;
                            Environment.SetEnvironmentVariable(prop.Name, envValue);
                        }
                    }
                }

                Assembly assembly = Assembly.LoadFrom(assemblyPath);
                Type = assembly.GetType(mainParts[1]);
                if (Type == null)
                {
                    return LogErrorToConsoleAndReturnErrorJson($"Unable to locate requested type (\"{mainParts[1]}\").");
                }
                Method = Type.GetMethod(mainParts[2]);
                Constructor = Type.GetConstructor(Type.EmptyTypes);
            }
            catch (Exception ex)
            {
                return LogErrorToConsoleAndReturnErrorJson(ex.ToString()
#if DEBUG
                                                          + ", " + ex.StackTrace
#endif
                );
            }

            if (Method == null)
            {
                return LogErrorToConsoleAndReturnErrorJson($"Unable to locate requested method (\"{mainParts[2]}\").");
            }

            if (Constructor == null)
            {
                return LogErrorToConsoleAndReturnErrorJson($"Unable to locate appropriate constructor for (\"{mainParts[1]}\").");
            }



            Initialized = true;
            AwaitableMethod = (Method.ReturnType.GetMethod(nameof(Task.GetAwaiter)) != null);

            return Results.Ok(new { Message = "Hello World", input = JsonSerializer.Serialize(initBody) });
        }
        catch (Exception ex)
        {
            WriteLogMarkers();
            return LogErrorToConsoleAndReturnErrorJson(ex.Message
#if DEBUG
                                                  + ", " + ex.StackTrace
#endif
                );
        }
        finally
        {
            _initSemaphoreSlim.Release();
        }
    }



    private static void WriteLogMarkers()
    {
        Console.WriteLine("XXX_THE_END_OF_A_WHISK_ACTIVATION_XXX");
        Console.Error.WriteLine("XXX_THE_END_OF_A_WHISK_ACTIVATION_XXX");
    }
}

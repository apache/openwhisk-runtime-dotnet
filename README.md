<!--
#
# Licensed to the Apache Software Foundation (ASF) under one or more
# contributor license agreements.  See the NOTICE file distributed with
# this work for additional information regarding copyright ownership.
# The ASF licenses this file to You under the Apache License, Version 2.0
# (the "License"); you may not use this file except in compliance with
# the License.  You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
#
-->

# Apache OpenWhisk runtimes for .NET Core

[![License](https://img.shields.io/badge/license-Apache--2.0-blue.svg)](http://www.apache.org/licenses/LICENSE-2.0)
[![Build Status](https://travis-ci.com/apache/openwhisk-runtime-dotnet.svg?branch=master)](https://travis-ci.com/github/apache/openwhisk-runtime-dotnet)
## Give it a try today

Create a C# project called Apache.OpenWhisk.Example.Dotnet:

```bash
dotnet new classlib -n Apache.OpenWhisk.Example.Dotnet -lang "C#"
cd Apache.OpenWhisk.Example.Dotnet
```

Install the [Newtonsoft.Json](https://www.newtonsoft.com/json) NuGet package as follows:

```bash
dotnet add package Newtonsoft.Json -v 12.0.1
```

Now create a file called `Hello.cs` with the following content:

```csharp
using System;
using Newtonsoft.Json.Linq;

namespace Apache.OpenWhisk.Example.Dotnet
{
    public class Hello
    {
        public JObject Main(JObject args)
        {
            string name = "stranger";
            if (args.ContainsKey("name")) {
                name = args["name"].ToString();
            }
            JObject message = new JObject();
            message.Add("greeting", new JValue($"Hello, {name}!"));
            return (message);
        }
    }
}
```
Publish the project as follows:

```bash
dotnet publish -c Release -o out
```

Zip the published files as follows:

```bash
cd out
zip -r -0 helloDotNet.zip *
```

Create the action

```bash
wsk action update helloDotNet helloDotNet.zip --main Apache.OpenWhisk.Example.Dotnet::Apache.OpenWhisk.Example.Dotnet.Hello::Main --kind dotnet:2.2
```

For the return result, not only support `dictionary` but also support `array`

So a very simple `hello array` function would be:

```csharp
using System;
using Newtonsoft.Json.Linq;

namespace Apache.OpenWhisk.Tests.Dotnet
{
    public class HelloArray
    {
        public JArray Main(JObject args)
        {
            JArray jarray = new JArray();
            jarray.Add("a");
            jarray.Add("b");
            return (jarray);
        }
    }
}
```

And support array result for sequence action as well, the first action's array result can be used as next action's input parameter.

So the function can be:

```csharp
using System;
using Newtonsoft.Json.Linq;

namespace Apache.OpenWhisk.Tests.Dotnet
{
    public class HelloPassArrayParam
    {
        public JArray Main(JArray args)
        {
            return (args);
        }
    }
}
```

## Changelogs

- [.NET Core 2.2 CHANGELOG.md](core/dotnet2.2/CHANGELOG.md)
- [.NET Core 3.1 CHANGELOG.md](core/dotnet3.1/CHANGELOG.md)

## Quick Start Guides

- [.NET Core 2.2](core/dotnet2.2/QUICKSTART.md)
- [.NET Core 3.1](core/dotnet3.1/QUICKSTART.md)

# License

[Apache 2.0](LICENSE.txt)

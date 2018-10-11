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

[![Build Status](https://travis-ci.org/apache/incubator-openwhisk-runtime-dotnet.svg?branch=master)](https://travis-ci.org/apache/incubator-openwhisk-runtime-dotnet)

## Changelogs
- [.NET Core 2.1 CHANGELOG.md](core/dotnet2.1/CHANGELOG.md)


## Quick .NET Core Action
A .NET Core action is a .NET Core class library with a method called `Main` that has the exact signature as follows:

```csharp
public Newtonsoft.Json.Linq.JObject Main(Newtonsoft.Json.Linq.JObject);
```

In order to compile, test and archive .NET Core projects, you must have the [.NET Core SDK](https://www.microsoft.com/net/download) installed locally and the environment variable `DOTNET_HOME` set to the location where the `dotnet` executable can be found.

For example, create a C# project called `Apache.OpenWhisk.Example.Dotnet`:

```bash
dotnet new classlib -n Apache.OpenWhisk.Example.Dotnet -lang C#
cd Apache.OpenWhisk.Example.Dotnet
```

Install the [Newtonsoft.Json](https://www.newtonsoft.com/json) NuGet package as follows:

```bash
dotnet add package Newtonsoft.Json -v 11.0.2
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
zip -r -0 helloDotNet.bin *
```

You need to specify the name of the function handler using `--main` argument.
The value for `main` needs to be in the following format:
`{Assembly}::{Class Full Name}::{Method}`, e.q.,
`Apache.OpenWhisk.Example.Dotnet::Apache.OpenWhisk.Example.Dotnet.Hello::Main`

### Create the .NET Core Action
To use on a deployment of OpenWhisk that contains the runtime as a kind:
```bash
wsk action update helloDotNet helloDotNet.bin --main Apache.OpenWhisk.Example.Dotnet::Apache.OpenWhisk.Example.Dotnet.Hello::Main --kind dotnet:2.1
```

### Invoke the .NET Core Action
Action invocation is the same for .NET Core actions as it is for Swift and JavaScript actions:

```bash
wsk action invoke --result helloDotNet --param name World
```

```json
  {
      "greeting": "Hello World!"
  }
```

## Local development
```bash
./gradlew core:dotnet2.1:distDocker
```
This will produce the image `whisk/action-dotnet-v2.1`

Build and Push image
```bash
docker login
./gradlew core:action-dotnet-v2.1:distDocker -PdockerImagePrefix=$prefix-user -PdockerRegistry=docker.io
```

Deploy OpenWhisk using ansible environment that contains the kind `dotnet:2.1`
Assuming you have OpenWhisk already deploy localy and `OPENWHISK_HOME` pointing to root directory of OpenWhisk core repository.

Set `ROOTDIR` to the root directory of this repository.

Redeploy OpenWhisk
```bash
cd $OPENWHISK_HOME/ansible
ANSIBLE_CMD="ansible-playbook -i ${ROOTDIR}/ansible/environments/local"
$ANSIBLE_CMD setup.yml
$ANSIBLE_CMD couchdb.yml
$ANSIBLE_CMD initdb.yml
$ANSIBLE_CMD wipe.yml
$ANSIBLE_CMD openwhisk.yml
```

Or you can use `wskdev` and create a soft link to the target ansible environment, for example:
```
ln -s ${ROOTDIR}/ansible/environments/local ${OPENWHISK_HOME}/ansible/environments/local-dotnet
wskdev fresh -t local-dotnet
```

### Testing
Install dependencies from the root directory on $OPENWHISK_HOME repository
```bash
pushd $OPENWHISK_HOME
./gradlew install
podd $OPENWHISK_HOME
```

Using gradle to run all tests
```bash
./gradlew :tests:test
```
Using gradle to run some tests
```bash
./gradlew :tests:test --tests *ActionContainerTests*
```
Using IntelliJ:
- Import project as gradle project.
- Make sure working directory is root of the project/repo

#### Using container image to test
To use as docker action push to your own dockerhub account
```bash
docker tag whisk/action-dotnet-v2.1 $user_prefix/action-dotnet-v2.1
docker push $user_prefix/action-dotnet-v2.1
```
Then create the action using your the image from dockerhub
```bash
wsk action update helloDotNet helloDotNet.bin --main Apache.OpenWhisk.Example.Dotnet::Apache.OpenWhisk.Example.Dotnet.Hello::Main --docker $user_prefix/action-dotnet-v2.1
```
The `$user_prefix` is usually your dockerhub user id.

# License
[Apache 2.0](LICENSE.txt)

# Disclaimer

Apache OpenWhisk Runtime Dotnet is an effort undergoing incubation at The Apache Software Foundation (ASF), sponsored by the Apache Incubator. Incubation is required of all newly accepted projects until a further review indicates that the infrastructure, communications, and decision making process have stabilized in a manner consistent with other successful ASF projects. While incubation status is not necessarily a reflection of the completeness or stability of the code, it does indicate that the project has yet to be fully endorsed by the ASF.

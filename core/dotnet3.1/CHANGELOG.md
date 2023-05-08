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

# .NET Core 3.1 OpenWhisk Runtime Container

## 1.17
-  Support array result include sequence action (#65)

## 1.16
- Fix Akka version ($55)
- Init json quickstart updates (#50)
- Export init args to environment. (#44)
- Upgrade dotnet sdk packages (removed 2.2 references) (#42)

## 1.15
Changes:
- Increased MaxRequestBodySize, so larger zip files can be uploaded (#33)
- Get the latest security fixes (apk upgrade) with every build.

## 1.14
Changes:
- Initial release
- Support for async methods. Example:

```csharp
        public async Task<JObject> MainAsync(JObject args)
        {
            await Task.Delay(10); // Just do a delay to have an async/await process occur.
            return (args);
        }
```

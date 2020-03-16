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

# .NET Core 2.2 OpenWhisk Runtime Container


## 1.15 (next release)
Changes:
- Get the latest security fixes (apk upgrade) with every build.


## 1.14
Changes:
- Support for async methods. Example:

```csharp
        public async Task<JObject> MainAsync(JObject args)
        {
            await Task.Delay(10); // Just do a delay to have an async/await process occur.
            return (args);
        }
```

## 1.13
Changes:
- Initial release

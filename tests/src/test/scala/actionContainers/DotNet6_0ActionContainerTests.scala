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

package actionContainers

import org.junit.runner.RunWith
import org.scalatest.junit.JUnitRunner
import common.WskActorSystem
import spray.json._
import actionContainers.ActionContainer.withContainer
import java.nio.file.Paths

@RunWith(classOf[JUnitRunner])
class DotNet3_1ActionContainerTests extends BasicActionRunnerTests with WskActorSystem {
  val functionb64 = ResourceHelpers.readAsBase64(Paths.get(getClass.getResource("/dotnettests6.0.zip").getPath))

  // Helpers specific to java actions
  override def withActionContainer(env: Map[String, String] = Map.empty)(
    code: ActionContainer => Unit): (String, String) = withContainer("action-dotnet-v6.0", env)(code)

  behavior of "dotnet action"

  override val testNoSourceOrExec = {
    TestConfig("")
  }

  override val testNotReturningJson = {
    // skip this test since and add own below (see Nuller)
    TestConfig("", skipTest = true)
  }

  override val testEnv = {
    TestConfig(functionb64, main = "Apache.OpenWhisk.Tests.Dotnet::Apache.OpenWhisk.Tests.Dotnet.Environment::Main")
  }

  override val testEnvParameters = {
    TestConfig(functionb64, main = "Apache.OpenWhisk.Tests.Dotnet::Apache.OpenWhisk.Tests.Dotnet.Init::Main")
  }

  override val testEcho = {
    TestConfig(functionb64, main = "Apache.OpenWhisk.Tests.Dotnet::Apache.OpenWhisk.Tests.Dotnet.AltEcho::Main")
  }

  val testEchoNoWrite = {
    TestConfig(functionb64, main = "Apache.OpenWhisk.Tests.Dotnet::Apache.OpenWhisk.Tests.Dotnet.Echo::MainAsync")
  }

  override val testUnicode = {
    TestConfig(functionb64, main = "Apache.OpenWhisk.Tests.Dotnet::Apache.OpenWhisk.Tests.Dotnet.Unicode::Main")
  }

  override val testInitCannotBeCalledMoreThanOnce = testEchoNoWrite

  override val testEntryPointOtherThanMain = testEchoNoWrite

  override val testLargeInput = testEchoNoWrite

  it should "fail to initialize with bad archive" in {
    val (out, err) = withActionContainer() { c =>
      val brokenArchive = ("NOTAVALIDZIPFILE")

      val (initCode, initRes) =
        c.init(initPayload(brokenArchive, "Apache.OpenWhisk.Tests.Dotnet::Apache.OpenWhisk.Tests.Dotnet.Invalid::Main"))
      initCode should not be (200)

      initRes shouldBe defined

      initRes should {
        be(Some(JsObject("error" -> JsString("Unable to decompress package."))))
      }
    }
  }

  it should "return some error on action error" in {
    val (out, err) = withActionContainer() { c =>
      val (initCode, _) =
        c.init(initPayload(functionb64, "Apache.OpenWhisk.Tests.Dotnet::Apache.OpenWhisk.Tests.Dotnet.Exception::Main"))
      initCode should be(200)

      val (runCode, runRes) = c.run(runPayload(JsObject.empty))
      runCode should not be (200)

      runRes shouldBe defined
      runRes.get.fields.get("error") shouldBe defined
    }

    checkStreams(out, err, {
      case (o, e) =>
        (o + e).toLowerCase should include("exception")
    })
  }

  it should "support a large payload" in {
    val (out, err) = withActionContainer() { c =>
      val payload = functionb64 + (" " * 18000000)
      val (initCode, _) =
        c.init(initPayload(payload, "Apache.OpenWhisk.Tests.Dotnet::Apache.OpenWhisk.Tests.Dotnet.Error::Main"))
      initCode should be(200)
    }
  }

  it should "support application errors" in {
    val (out, err) = withActionContainer() { c =>
      val (initCode, _) =
        c.init(initPayload(functionb64, "Apache.OpenWhisk.Tests.Dotnet::Apache.OpenWhisk.Tests.Dotnet.Error::Main"))
      initCode should be(200)

      val (runCode, runRes) = c.run(runPayload(JsObject.empty))
      runCode should be(200)

      runRes shouldBe defined

      runRes should {
        be(Some(JsObject("error" -> JsString(".NETCoreApp,Version=v6.0"))))
      }
    }

    checkStreams(out, err, {
      case (o, e) =>
        o shouldBe empty
        e shouldBe empty
    })
  }

  it should "fails on invalid assembly reference" in {
    val (out, err) = withActionContainer() { c =>
      val (initCode, initRes) = c.init(
        initPayload(functionb64, "Apache.OpenWhisk.Tests.Dotnet.DoesntExist::Apache.OpenWhisk.Tests.Dotnet.Echo::Main"))
      initCode should be(502)

      initRes shouldBe defined

      initRes should {
        be(
          Some(JsObject("error" -> JsString(
            "Unable to locate requested assembly (\"Apache.OpenWhisk.Tests.Dotnet.DoesntExist.dll\")."))))
      }
    }
  }

  it should "fails on invalid type reference" in {
    val (out, err) = withActionContainer() { c =>
      val (initCode, initRes) =
        c.init(initPayload(functionb64, "Apache.OpenWhisk.Tests.Dotnet::Apache.OpenWhisk.Tests.Dotnet.FakeType::Main"))
      initCode should be(502)

      initRes should {
        be(
          Some(JsObject(
            "error" -> JsString("Unable to locate requested type (\"Apache.OpenWhisk.Tests.Dotnet.FakeType\")."))))
      }
    }
  }

  it should "fails on invalid method reference" in {
    val (out, err) = withActionContainer() { c =>
      val (initCode, initRes) = c.init(
        initPayload(functionb64, "Apache.OpenWhisk.Tests.Dotnet::Apache.OpenWhisk.Tests.Dotnet.Echo::FakeMethod"))
      initCode should be(502)

      initRes should {
        be(Some(JsObject("error" -> JsString("Unable to locate requested method (\"FakeMethod\")."))))
      }
    }
  }

  it should "fails on type with no empty constructor" in {
    val (out, err) = withActionContainer() { c =>
      val (initCode, initRes) = c.init(
        initPayload(
          functionb64,
          "Apache.OpenWhisk.Tests.Dotnet::Apache.OpenWhisk.Tests.Dotnet.NonEmptyConstructor::Main"))
      initCode should be(502)

      initRes should {
        be(
          Some(JsObject("error" -> JsString(
            "Unable to locate appropriate constructor for (\"Apache.OpenWhisk.Tests.Dotnet.NonEmptyConstructor\")."))))
      }
    }
  }

  it should "validate main string format 1" in {
    val (out, err) = withActionContainer() { c =>
      val (initCode, initRes) = c.init(initPayload(functionb64, "Apache.OpenWhisk.Tests.Dotnet"))
      initCode should not be (200)

      initRes should {
        be(Some(JsObject("error" -> JsString("main required format is \"Assembly::Type::Function\"."))))
      }
    }
  }

  it should "validate main string format 2" in {
    val (out, err) = withActionContainer() { c =>
      val (initCode, initRes) =
        c.init(initPayload(functionb64, "Apache.OpenWhisk.Tests.Dotnet::Apache.OpenWhisk.Tests.Dotnet.Echo"))
      initCode should not be (200)

      initRes should {
        be(Some(JsObject("error" -> JsString("main required format is \"Assembly::Type::Function\"."))))
      }
    }
  }

  it should "enforce that the user returns an object" in {
    val (out, err) = withActionContainer() { c =>
      val (initCode, _) =
        c.init(initPayload(functionb64, "Apache.OpenWhisk.Tests.Dotnet::Apache.OpenWhisk.Tests.Dotnet.Nuller::Main"))
      initCode should be(200)

      val (runCode, runRes) = c.run(runPayload(JsObject.empty))
      runCode should not be (200)

      runRes shouldBe defined
      runRes.get.fields.get("error") shouldBe defined
    }

    checkStreams(out, err, {
      case (o, e) =>
        (o + e).toLowerCase should include("the action returned null")
    })
  }
}

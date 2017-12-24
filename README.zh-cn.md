# ServiceAnt

ServiceAnt 的定位是服务总线，好吧，虽然它现在还没有实现真正意义上的服务总线，但正朝着那个方向发展。
目前它只能运行于进程内，不久会推出分布式的实现。现在你可以把它当作一个解耦模块的中介者（类似于 Mediator）

## Get Started

## 安装

`Instal-Package ServiceAnt`

## 使用

ServiceAnt 有两种工作模式分别是:

* Pub/Sub(发布/订阅)模式
* Req/Resp(请求/响应)模式

### Pub/Sub

该模式没有什么特别，完全遵从于观察者模式，当按这种方式使用时，你可以把 ServiceAnt 看作事件总线。
示例代码片段如下：

  Demo Code
  Demo Code
  Demo Code
  
  
### Req/Resp

该模式在正常的请求响应模式下追加了管道处理机制，有点类似于 `1Owin 的中间件` 与 `WebApi 的 MessageHandler` 它们的工作方式，不同的地方在于它是单向管道，
而后者是双向的
实例代码片段如下:

  DemoCode
  DemoCode
  DemoCode
  
## 注册处理函数

ServiceAnt 支持以下两种方式绑定处理函数

* 注册委托
* Ioc注册

### 显式注册委托

这种注册方式已经在之前的代码片段中演示过了。
值得一提的是，虽然我更推荐通过Ioc来注册处理函数（这样可以获得更好的可读性与可修改性以及可测试性），
但在我们团队使用过程并没有发现通过委托注册存在什么弊端，而且在你需要复用某些局部变量时，
这种方式会更好一些。

### Ioc注册

在使用Ioc注册之前，首先我们需要把 ServiceAnt 集成到你的 Ioc环境中，请参考 Ioc集成 来将 ServiceAnt 集成到你的 Ioc当中。

注册事件处理函数:

  Democode
  DemoCode
  DemoCode
  
注册请求处理函数:

  DemoCode
  DemoCode
  DemoCode

## Ioc 集成

ServiceAnt 可以直接开箱即用，但我相信很多使用者都会希望使用 Ioc 来注册自己的事件处理函数，这样做除了便于事件处理函数的单元测试外，同时也提高了可修改性。
目前 ServiceAnt 提供了以下几种DI框架的集成：

* AutoFac
* Castle

如果有你正在使用却没有实现的DI，欢迎在 Issue 里提出，我会找时间实现，或者实现后给我PR.

### Autofac

首先你需要安装 ServiceAnt 的 Autofac 集成:

`Install-Package ServiceAnt.Installer.Autofac`

然后在你的初始化代码中调用：

  DemoCode
  DemoCode
  
好的，你现在可以在你的服务中注入 ServiceAnt 了。

### Castle

类似于 Autofac , 先安装 Castle 的 Installer:

`Install-Package ServiceAnt.Installer.Castle`

然后在你的初始化代码中调用：

  DemoCode
  DemoCode

在进行完 Ioc 集成后，你就可以通过 DI 的方式注册处理函数了。

# ServiceAnt

[![Build status](https://ci.appveyor.com/api/projects/status/github/ShiningRush/serviceant?branch=master&svg=true)](https://ci.appveyor.com/project/ShiningRush/serviceant)
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/ShiningRush/ServiceAnt/blob/master/LICENSE)
  
[中文介绍请点击这里](https://github.com/ShiningRush/PdfComponentComparition/blob/master/README.zh-cn.md)
<br/>
ServiceAnt 的定位是一个轻量级，开箱即用的服务总线，好吧，虽然它现在还没有实现真正意义上的服务总线，但正朝着那个方向发展。
目前它只能运行于进程内，以后会推出分布式的实现。现在你可以把它当作一个解耦模块的中介者（类似于 Mediator）  
<br/>
[了解ServiceAnt为什么而出现](#Detail)
  
## Get Started

## 安装

`Install-Package ServiceAnt`

## 使用

ServiceAnt 有两种工作模式分别是:

* Pub/Sub(发布/订阅)模式
* Req/Resp(请求/响应)模式

### Pub/Sub

该模式没有什么特别，完全遵从于观察者模式，当按这种方式使用时，你可以把 ServiceAnt 看作事件总线。  
示例代码片段如下：
  
        static void Main(string[] args)
        {
            var serviceBus = InProcessServiceBus.Default;
            serviceBus.AddSubscription<TestEvent>((eventParam) =>
            {
                Console.WriteLine($"Subscription Handler get value: {eventParam.EventValue}");

                return Task.FromResult(0);
            });

            // it used when you do not want to create event class, you can handle it with a dynamic parameter
            serviceBus.AddDynamicSubscription("TestEvent", (eventParam) =>
            {
                Console.WriteLine($"DynamicSubscription Handler get value: {eventParam.EventValue}");

                return Task.FromResult(0);
            });

            var publishEvent = new TestEvent() { EventValue = "HelloWorld" };
            Console.WriteLine($"Publish event value: { publishEvent.EventValue }");
            serviceBus.Publish(publishEvent);
            Console.ReadLine();
        }

        class TestEvent : TransportTray
        {
            public string EventValue { get; set; }
        }
  
### Req/Resp

该模式在正常的请求响应模式下追加了管道处理机制，有点类似于 `1Owin 的中间件` 与 `WebApi 的 MessageHandler` 它们的工作方式，不同的地方在于它是单向管道，而后者是双向的.  
示例代码片段如下:

        static void Main(string[] args)
        {
            var serviceBus = InProcessServiceBus.Default;
            serviceBus.AddRequestHandler<TestRequest>((requestParam, handlerContext) =>
            {
                Console.WriteLine($"Request Handler get value: {requestParam.RequestParameter}");
                handlerContext.Response = "First handler has handled. \r\n";
                return Task.FromResult(0);
            });

            // it used when you do not want to create transporttray class, you can handle it with a dynamic parameter
            serviceBus.AddDynamicRequestHandler("TestRequest", (eventParam, handlerContext) =>
            {
                Console.WriteLine($"DynamicRequest Handler get value: {eventParam.RequestParameter}");
                handlerContext.Response += "Second handler has handled. \r\n";

                // set IsEnd flag to true then directly return response and ignore the rest handlers
                handlerContext.IsEnd = true;
                return Task.FromResult(0);
            });

            // this handler will not be excuted
            serviceBus.AddRequestHandler<TestRequest>((requestParam, handlerContext) =>
            {
                Console.WriteLine($"Third Request Handler get value: {requestParam.RequestParameter}");
                handlerContext.Response += "Third handler has handled. \r\n";
                return Task.FromResult(0);
            });

            var publishEvent = new TestRequest() { RequestParameter = "HelloWorld" };
            Console.WriteLine($"Send request parameter value: { publishEvent.RequestParameter }");
            var response = serviceBus.Send<string>(publishEvent);
            Console.WriteLine("The response is : \r\n" + response);

            Console.ReadLine();
        }

        class TestRequest : TransportTray
        {
            public string RequestParameter { get; set; }
        }
  
## 注册处理函数

ServiceAnt 支持以下两种方式注册处理函数

* 注册委托
* Ioc注册

### 注册委托

这种注册方式已经在之前的代码片段中演示过了, 它支持两种不同输入参数的委托, 一种是显式的泛型, 还有隐式的动态类型,  
(注意: 隐式的动态类型现在没法正确注册泛型的TransportTray, 因为泛型在转换名称的过程中不是单纯的类名转换)
值得一提的是，虽然我更推荐通过Ioc来注册处理函数（这样可以获得更好的可读性与可修改性以及可测试性）,   
但在我们团队使用过程中并没有发现通过委托注册存在什么弊端，而且在你需要复用某些局部变量时, 这种方式会更好一些。

### Ioc注册

在使用Ioc注册之前，首先我们需要把 ServiceAnt 集成到你的 Ioc环境中，请参考 [Ioc集成](#IocIntegration) 来将 ServiceAnt 集成到你的 Ioc当中。  

注册事件处理函数:

        public class IocEventHandler : IEventHandler<TestTray>
        {
            public Task HandleAsync(TestTray param)
            {
                RESULT_CONTAINER = param.Result;

                return Task.Delay(1);
            }
        }
  
注册请求处理函数:

        public class IocRequestHandler : IRequestHandler<TestTray>
        {
            public Task HandleAsync(TestTray param, IRequestHandlerContext handlerContext)
            {
                handlerContext.Response = param.Result;
                return Task.Delay(1);
            }
        }

<h2 id="IocIntegration"> Ioc 集成 </h2>

ServiceAnt 可以开箱即用，但我相信很多使用者都会希望使用 Ioc 来注册自己的事件处理函数，这样做除了便于事件处理函数的单元测试外，同时也提高了可修改性。  
目前 ServiceAnt 提供了以下几种Ioc框架的集成：

* Autofac
* Castle Windsor

如果有你正在使用却没有实现的Ioc框架，欢迎在 Issue 里提出，我会找时间实现，或者你可以实现以后给我PR.

### Autofac

首先你需要安装 ServiceAnt 的 Autofac 集成:

`Install-Package ServiceAnt.IocInstaller.Autofac`

然后在你的初始化代码中调用：


            // replace newContainer with your project container builder
            var newContainerBuilder = new ContainerBuilder();
            
            // input all of your assemblies containg handler, installer will automatically register it to container
            newContainerBuilder.RegisterModule(new ServiceAntModule(System.Reflection.Assembly.GetExecutingAssembly()));
            
            // after builded container, you should excute ServiceAntModule.RegisterHandlers to integrate ioc
            var autofacContainer = newContainerBuilder.Build();
            ServiceAntModule.RegisterHandlers(autofacContainer);
  
好的，你现在可以在你的服务中注入 ServiceAnt 了。

### Castle

类似于 Autofac , 先安装 Castle 的 Installer:

`Install-Package ServiceAnt.IocInstaller.Castle`

然后在你的初始化代码中调用：

            // replace newContainer with your project container
            var newContainer = new WindsorContainer();
            
            // input all of your assemblies containg handler, installer will automatically register it to container
            newContainer.Install(new ServiceAntInstaller(System.Reflection.Assembly.GetExecutingAssembly()));

在进行完 Ioc 集成后，你就可以通过 Ioc 的方式注册处理函数了。

<h2 id="Detail">为什么会有ServiceAnt</h2>

### 动机

起因是这样的，我们团队在开发一个企业应用时采用了DDD，然后将我们的业务逻辑拆分为了复数个限界上下文，每个上下文低耦合高内聚的.  
但无论再怎么低耦合，总会有一些高层次的交互，这些被称为“边界点”，通常在分布式部署中，我们会选择Webapi 或者 WebServie 等远程通信手段来进行交互  
遗憾的是，我们的应用是线下的，并发量也并不需要到集群这样重量级的解决方案，所以我们使用Abp的插件加载机制为基础设施,  
将每个上下文都实现成了一个个独立的项目模块.  
  
项目初期我们使用 Abp 提供的事件总线作为模块之间交互的方式, 但它有一个很不好的地方是, 它的事件引用必须是显式的原对象引用。    
这也就意味着，你为了在A模块中使用B模块发布的事件，你必须让两个上下文都引用这个事件对象，这显然加深了模块间的耦合。  
  
在参考了Abp, Medirator, NServerBus以及微软的示例项目 EShopContainer 我决定自己实现一个服务总线, 它要具有以下特点：
* 支持隐式注册处理函数
* 支持Pub/Sub模式
* 事件的接收与发布对象是非引用的(指你可以在不同模块间建立各自的事件类，只需要保证它们名称与结构相同即可)

  
所以ServiceAnt出现了, ServiceAnt 的初期目标是一个进程内的消息中介者, 后期有时间会开发分布式的版本。

### 与其他类似框架的不同之处

`Mediator`: Mediator 只支持Ioc来注册处理函数, 并且不支持委托注册  
`NServerBus`: NServerBus 是一个偏重的框架, 而且它的定位就是解决分布式架构中的通信问题，并没有进程内的实现版本(它的Learning Transport可以看作进程内的实现，但官方并不推荐使用)  
`Abp`: 在上面已经讨论过了, 而且它也不支持 Pub/Sub  
`EShopContainer`: 好吧，这只是微软的实例项目，它其中的事件总线是分布式的，有两个实现，一个基于RabbitMQ一个基于AzureMQ, 它也没有作为框架发布到Nuget上  

# ServiceAnt

[![Build status](https://ci.appveyor.com/api/projects/status/github/ShiningRush/serviceant?branch=master&svg=true)](https://ci.appveyor.com/project/ShiningRush/serviceant)
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/ShiningRush/ServiceAnt/blob/master/LICENSE)

[中文介绍请点击这里](https://github.com/ShiningRush/PdfComponentComparition/blob/master/README.zh-cn.md)  

ServiceAnt 的定位是一个轻量级，开箱即用的服务总线，好吧，虽然它现在还没有实现真正意义上的服务总线，但正朝着那个方向发展。  
目前它只能运行于进程内，以后会推出分布式的实现。现在你可以把它当作一个解耦模块的中介者（类似于 Mediator）  

[了解ServiceAnt为什么而出现](#Detail)
  
## Get Started

## 安装

```
Install-Package ServiceAnt
```

## 使用

ServiceAnt 有两种工作模式分别是:

* Pub/Sub(发布/订阅)模式
* Req/Resp(请求/响应)模式

### Pub/Sub

该模式没有什么特别，完全遵从于观察者模式，当按这种方式使用时，你可以把 ServiceAnt 看作事件总线。  

示例代码片段如下：
```c#
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

        class TestEvent : IEventTrigger
        {
            public string EventValue { get; set; }
        }
```

### Req/Resp

该模式在正常的请求响应模式下追加了管道处理机制，有点类似于 `Owin 的中间件` 与 `WebApi 的 MessageHandler` 它们的工作方式，不同的地方在于它是单向流动的，而后者是双向的.  

> ### 管道的处理顺序
> 管道处理顺序是按照的注册的顺序来执行的，暂时没有提供控制执行顺序的配置.  
> 在大多数情况下，我们只推荐为 Req/Resp 注册单个注册函数.

示例代码片段如下:
```c#
        static void Main(string[] args)
        {
            var serviceBus = InProcessServiceBus.Default;
            serviceBus.AddRequestHandler<TestRequest>((requestParam, handlerContext) =>
            {
                Console.WriteLine($"Request Handler get value: {requestParam.RequestParameter}");
                handlerContext.Response = "First handler has handled. \r\n";
                return Task.FromResult(0);
            });

            // it used when you do not want to create trigger class, you can handle it with a dynamic parameter
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

        class TestRequest : IRequestTrigger
        {
            public string RequestParameter { get; set; }
        }
```

> ### 提示
> 在使用 Pub/Sub 模式时, 你的触发对象必须继承 IEventTrigger 接口, 而 Req/Resp 模式则必须继承 IRequestTrigger.

## 注册处理函数

ServiceAnt 支持以下两种方式注册处理函数

* 注册委托
* Ioc注册

### 注册委托

这种注册方式已经在之前的代码片段中演示过了, 它支持两种不同输入参数的委托, 一种是显式的泛型, 还有隐式的动态类型.  

值得一提的是，虽然我更推荐通过Ioc来注册处理函数（这样可以获得更好的可读性与可修改性以及可测试性）,  
但在我们团队使用过程中并没有发现通过委托注册存在什么弊端，而且在你需要复用某些局部变量时, 这种方式会更好一些。  

> ### 注意
> 隐式的动态类型现在没法正确注册泛型的 Trigger 的处理函数, 因为泛型在转换名称的过程中不是单纯的类名转换

### Ioc注册

在使用Ioc注册之前，首先我们需要把 ServiceAnt 集成到你的 Ioc环境中，请参考 [Ioc集成](#IocIntegration) 来将 ServiceAnt 集成到你的 Ioc当中。  

注册事件处理函数:
```c#
        public class IocEventHandler : IEventHandler<TestEventTrigger>
        {
            public Task HandleAsync(TestTray param)
            {
                RESULT_CONTAINER = param.Result;

                return Task.Delay(1);
            }
        }
```  

注册请求处理函数:
```c#
        public class IocRequestHandler : IRequestHandler<TestRequestTrigger>
        {
            public Task HandleAsync(TestTray param, IRequestHandlerContext handlerContext)
            {
                handlerContext.Response = param.Result;
                return Task.Delay(1);
            }
        }
```
<h2 id="IocIntegration"> Ioc 集成 </h2>

ServiceAnt 可以开箱即用，但我相信很多使用者都会希望使用 Ioc 来注册自己的事件处理函数，这样做除了便于事件处理函数的单元测试外，同时也提高了可修改性。   

目前 ServiceAnt 提供了以下几种Ioc框架的集成：

* Autofac
* Castle Windsor
* DotNet Core

如果有你正在使用却没有实现的Ioc框架，欢迎在 Issue 里提出，我会找时间实现，或者你可以实现以后给我PR.

### Autofac

首先你需要安装 ServiceAnt 的 Autofac 集成:

```
Install-Package ServiceAnt.IocInstaller.Autofac
```

然后在你的初始化代码中调用：

```c#
            // replace newContainer with your project container builder
            var newContainerBuilder = new ContainerBuilder();
            
            // input all of your assemblies containg handler, installer will automatically register it to container
            newContainerBuilder.RegisterModule(new ServiceAntModule(System.Reflection.Assembly.GetExecutingAssembly()));
```

好的，你现在可以在你的Ioc环境中使用 ServiceAnt 了.  

> ### 关于处理函数(Handler)的自动注册
> 需要注意一下 `ServiceAntModule` 的构造函数可以接受多个程序集, 这些程序应该包含你的处理函数(Handler)所在的程序集,  
> 安装器会帮你自动把处理函数(Handler)注册到Ioc的容器中, 同时也会自动添加到 `IServiceBus` 中.  
> 
> 在一些复杂的模块化系统可能在初始化模块中找出所有处理函数的程序集会比较麻烦, 但遗憾的是 `Autofac` 不象 `Castle.Windsor` 一样支持拓展的钩子,
> 所以没办法在注册时依赖时自动注册到 `IServiceBus` 中, 目前我们正在研究是否有更合适的解决方案.  

### Castle.Windsor

类似于 Autofac , 先安装 Castle 的 Installer:

```
Install-Package ServiceAnt.IocInstaller.Castle
```

然后在你的初始化代码中调用：

```c#
            // replace newContainer with your project container
            var newContainer = new WindsorContainer();
            
            // input all of your assemblies containg handler, installer will automatically register it to container
            newContainer.Install(new ServiceAntInstaller(System.Reflection.Assembly.GetExecutingAssembly()));
```

Castle 的安装器也支持在构造函数中放入处理函数(Handler)所在的程序集, 它会自动帮你注册到 Ioc 容器和 `IServiceBus` 中,  
但和 Autofac 有一点很大不同的是, Castle的容器支持一些注册时的钩子, 它可以让安装器在注册依赖时自动帮你把处理函数(Handler)注册到`IServiceBus`.  
这样一来, 如果你的安装器是在启动模块中安装的, 那么你可以不用关心你的处理函数处于哪个程序集了, 只要你在任意模块把它注册到你 Ioc 的容器中,  
ServiceAnt都会感知到, 并且自动添加它.  

请看下面的示例代码.  
初始化模块:  
```c#
            // replace newContainer with your project container
            var newContainer = new WindsorContainer();
            
            // you dont need to input assmblies which contains handler function
            newContainer.Install(new ServiceAntInstaller());
```

模块A(在初始化模块执行后):  
```c#
            // register your component to container, hook function will automatically add it to IServiceBus
            _container.Register(Component.For<IEventHandler>().ImplementedBy<SomeConcreteEventHandler>());
```

以上的代码可以即可将事件处理函数以Ioc的方式注册到 `IServiceBus` 中.

### DotNetCore

ServiceAnt 也支持 `Standard2.0` 所以你可以在 .netcore2.0 以上的版本中使用 ServiceAnt,  
在 .net core中集成 ServiceAnt 需要先安装相关的集成包:  

```
Install-Package ServiceAnt.IocInstaller.DotNetCore
```

然后将它集成到你的应用程序中:  

```c#
        public void ConfigureServices(IServiceCollection services)
        {
            ...
            
            // installer will automaticlly register the handler function to servicebus
            services.AddServiceAnt(System.Reflection.Assembly.GetExecutingAssembly());
            
            ...
        }
```

## 异常处理与日志

### 异常处理
ServiceAnt 在触发处理函数的过程中,可能会产生某些异常,正常情况下我们希望用户能在自己的处理函数中干掉他们,  
但如果出现了用户未处理的异常, ServiceAnt 会采取以下的默认方式处理它们:  

* `Pub/Sub`:  所有异常会被捕捉, 并且记录日志消息, 但不会上抛, 也就是说某一个处理函数发生异常并不会影响其他订阅者的触发.  

* `Req/Resp`: 这种模式下的异常会记录日志消息, 并被上抛, 这会中断接下来的处理函数(如果有的话).

如果你需要更改它们的默认行为, 在触发相应的函数时你可以传入 `TriggerOption` 来控制是否忽略未处理的异常, 如下:   

```c#
          // it will make servicebus not ignore exception when handler function raise a unhanled exception
          serviceBus.Publish(testEventData, new TriggerOption(false));
```

### 日志

你如果订阅了位于 `IServiceBus` 中的 `OnLogBusMessage` 事件, 那么 `ServiceBus` 的日志消息都会通过该事件发出.  

```c#
            serviceBus.OnLogBusMessage += (logLevel, msg, ex) =>
            {
                logMsg = msg;
                catchedException = ex;
            };
```

## 使用ServiceAnt 的一些最佳实践

### Trigger 的命名规范

为了使参与开发的成员都能快速识别出 Trigger 的使用目的和选择的通信方式, 我们建议 Pub/Sub 的 Trigger 命名以 On 开头, 如：
```c#
        public class OnEntityHasChanged : IEventTrigger
        {
        }
```

而 Req/Resp 的 Trigger 以 Get 开头.  
```c#
        public class GetDataItemWithCode : IRequestTrigger
        {
        }
```

### 不要过于频繁地调用 ServiceBus

就算在同一进程内也不要过于频繁地调用 ServiceBus (比如在循环时), 在因为ServiceBus中为了解耦引用, 将对 Trigger 与返回值都进行了序列化, 如果调用过于频繁, 毫无疑问会带来一定的性能开支, 建议你把所需的内容一次性都获取到, 而不是等到遍历时再去获取.

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
* 支持 Req/Resp 模式
* 事件的接收与发布对象是非引用的(指你可以在不同模块间建立各自的事件类，只需要保证它们名称与结构相同即可)  

所以ServiceAnt出现了, ServiceAnt 的初期目标是一个进程内的消息中介者, 后期有时间会开发分布式的版本。

### 与其他类似框架的不同之处

这里只作一些不同之处的分析, 并不代表优劣, 请结合自己项目的情况来合理选择.

`Mediator`: Mediator 只支持Ioc来注册处理函数, 并且不支持委托注册. 另外它的定位是进程内使用的基础设施, 不适用于分布式系统, 在`eshopcontainer`中,它被作为单个微服务下实现 CQRS 的基础设施.

`NServerBus`: NServerBus 是一个偏重的框架, 而且它的定位就是解决分布式架构中的通信问题，并没有进程内的实现版本(它的Learning Transport可以看作进程内的实现，但官方并不推荐使用), 它更适用于分布式的复杂系统来说.

`Abp`: 在上面已经讨论过了, 另外它也不支持 Pub/Sub.如果你的项目不采用多模块的机制, 或者不介意模块间的相互引用, Abp自带的事件还是不错的.

`EShopContainer`: 这只是微软的示例项目，它其中的事件总线是分布式的，有两个实现，一个基于RabbitMQ一个基于AzureMQ, 它也没有作为框架发布到Nuget上.

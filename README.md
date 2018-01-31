# ServiceAnt

[![Build status](https://ci.appveyor.com/api/projects/status/github/ShiningRush/serviceant?branch=master&svg=true)](https://ci.appveyor.com/project/ShiningRush/serviceant)
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/ShiningRush/ServiceAnt/blob/master/LICENSE)
[![NuGet](https://img.shields.io/nuget/vpre/serviceant.svg)](https://www.nuget.org/packages/ServiceAnt)

[中文介绍请点击这里](https://github.com/ShiningRush/PdfComponentComparition/blob/master/README.zh-cn.md)  

ServiceAnt is a lightweight servicebus which is out-of-the-box. Well, though it can not be called servicebus but it will in soon.  
It is only running in-process system, but i will make it run distributed system in future.  
You can see it as a communication mediator in this version.

[Learn why serviceant is created](#Detail)
  
## Get Started

## Install

```
Install-Package ServiceAnt
```

## Usage

ServiceAnt has two mode of communication:

* Pub/Sub
* Req/Resp

### Pub/Sub

This mode has nothing special, it completely follow observer design pattern.  

You can see serviceant as a eventbus when you use this mode.

Sample code：
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

This mode appends pipeline handling in normal request-response mode, somewhat similar to `Owin's middleware` and `WebApi's MessageHandler`, which work differently, except that it is a one-way flow and the latter is bidirectional of. 

> ### The order of Pipeline processing
> Pipeline processing order is in accordance with the order of registration to perform, there is no provision for the control of the implementation of the order.  
> In most cases, we only recommend registering a single handler for Req / Resp.

Sample code:
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

> ### Tips
> When using Pub / Sub mode, your trigger object must inherit the IEventTrigger interface, while Req / Resp mode must inherit IRequestTrigger.

## Register handler

ServiceAnt supports registering handlers in the following two ways:

* Register with delegate
* Register with IOC

### Register with delegate

This registration method has been demonstrated in the previous code snippet, it supports two different input parameters of the delegate, one is an explicit generic, the other is a dynamic type  

It is worth mentioning that, although I also recommend registering handler with IOC (which can achieve better readability and modifiability and testability),  
However, We have not found any problems to register handler with delegate and when you need to reuse some of the local variables, this approach would be better。  

> ### Notice
> The registration of dynamic type parameters requires the event names, which may result in the failure to properly register certain generic Trigger handlers because generics are not simply class name conversions during name conversion.

### Register with IOC

Before using Ioc registration, we need to integrate ServiceAnt into your Ioc environment first. Please refer to [Ioc Integration](# IocIntegration) to integrate ServiceAnt into your Ioc.  

Register event handler:
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

Register request handler:
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
<h2 id="IocIntegration"> Ioc Integration </h2>

ServiceAnt can be used out of the box, but I believe many users will want to register their own event handler with ioc, in addition to facilitate unit testing of event handlers, but also improve the modifiability.   

ServiceAnt now provides the integration of several Ioc frameworks：

* Autofac
* Castle Windsor
* DotNet Core

If you have an unsupported Ioc framework that you are using, please feel free to ask in the issues, I will find time to implement, or you can implement it and give me a PR.

### Autofac

First you need to install ServiceAnt's Autofac integration:

```
Install-Package ServiceAnt.IocInstaller.Autofac
```

Then in your initialization code：

```c#
            // replace newContainer with your project container builder
            var newContainerBuilder = new ContainerBuilder();
            
            // input all of your assemblies containg handler, installer will automatically register it to container
            newContainerBuilder.RegisterModule(new ServiceAntModule(System.Reflection.Assembly.GetExecutingAssembly()));
```

Sure, you can now use ServiceAnt in your Ioc environment.  

> ### About the handler (Handler) automatic registration
> Note that the constructor of `ServiceAntModule` can accept multiple assemblies, and these should include the assembly where your handler is in,  
> The installer will automatically register your handler to Ioc's container and will also be automatically added to `IServiceBus`.  
> 
> In some complex modular systems, it is maybe difficult to find all assemblies which contains handlers in the startup module, but unfortunately `Autofac` does not support extended hooks like `Castle.Windsor`,
> So there is no way to automatically register to `IServiceBus` at registering handler to container, we are currently looking at whether there is a more suitable solution.  

### Castle.Windsor

Similar to Autofac, install Castle's Installer first:

```
Install-Package ServiceAnt.IocInstaller.Castle
```

Then call in your initialization code：

```c#
            // replace newContainer with your project container
            var newContainer = new WindsorContainer();
            
            // input all of your assemblies containg handler, installer will automatically register it to container
            newContainer.Install(new ServiceAntInstaller(System.Reflection.Assembly.GetExecutingAssembly()));
```

Castle's installer also supports accepting the assembly of handlers in the constructor, which will automatically register handler to Ioc containers and IServiceBus,  
But one big difference with `Autofac` is that Castle's container supports some registration hooks that allow the installer to automatically register your handler with IServiceBus when registering dependencies.  
As a result, if your installer is installed in the startup module, you do not have to worry about which assembly your handler is in, as long as you register it to your Ioc container in any module,
ServiceAnt will be aware of, and automatically add it.

Please see the following sample code:  
startup module:  
```c#
            // replace newContainer with your project container
            var newContainer = new WindsorContainer();
            
            // you dont need to input assmblies which contains handler function
            newContainer.Install(new ServiceAntInstaller());
```

ModuleA(after startup module executed):  
```c#
            // register your component to container, hook function will automatically add it to IServiceBus
            _container.Register(Component.For<IEventHandler>().ImplementedBy<SomeConcreteEventHandler>());
```

The above code can register the event handler to IServiceBus by Ioc.

### DotNetCore

ServiceAnt also supports `Standard2.0` so you can use ServiceAnt in versions above .netcore2.0,  
Integrating ServiceAnt in .net core requires that you install the related integration package first:  

```
Install-Package ServiceAnt.IocInstaller.DotNetCore
```

Then integrate it into your application:  

```c#
        public void ConfigureServices(IServiceCollection services)
        {
            ...
            
            // installer will automaticlly register the handler function to servicebus
            services.AddServiceAnt(System.Reflection.Assembly.GetExecutingAssembly());
            
            ...
        }
```

## Exception handling and log

### Exception handling
ServiceAnt may raise some exception when triggering event or request. Normally, we expect users to be able to handle them in their own handler,  
However, if a user unhandled exception occurs, ServiceAnt takes the following default approach to them:  

* `Pub/Sub`:  All exceptions will be caught, and log messages, but not rethrow, which means that a handler raising exception does not affect other handlers triggering.  

* `Req/Resp`: Exceptions in this mode will log messages and rethrow, and not execute the rest of handler.  

If you need to change their default behavior, you can pass `TriggerOption` to control whether ignore unhandled exceptions when triggering event or request:   

```c#
          // it will make servicebus not ignore exception when handler function raise a unhanled exception
          serviceBus.Publish(testEventData, new TriggerOption(false));
```

### Log

If you subscribe to the `OnLogBusMessage` event located in `IServiceBus`, the messages from `ServiceBus` will be sent out through this event.  

```c#
            serviceBus.OnLogBusMessage += (logLevel, msg, ex) =>
            {
                logMsg = msg;
                catchedException = ex;
            };
```

## Some best practices for using ServiceAnt

### Trigger naming conventions

In order for team developer to quickly realize the purpose of use of the Trigger and the communication mode, we recommend that naming the trigger of the Pub / Sub start with On, as：
```c#
        public class OnEntityHasChanged : IEventTrigger
        {
        }
```

and the trigger of Req/Resp start with Get.  
```c#
        public class GetDataItemWithCode : IRequestTrigger
        {
        }
```

### Do not call ServiceBus frequently

Do not call ServiceBus frequently even ServiceAnt work in in-process enviroment, such as in the loop, because ServiceBus in order to decouple the reference, the trigger and return values are serialized, if the call is too frequent, no doubt to bring To a certain performance costs, it is recommended that you get all the required content at once, rather than wait until the traversal to obtain.

<h2 id="Detail">Why ServiceAnt</h2>

### Motivation

The reason is that our team used DDD when developing an enterprise application and then split our business logic into a number of bounding contexts with low in coupling and high in cohesion per context.  

但无论再怎么低耦合，总会有一些高层次的交互，这些被称为“边界点”，通常在分布式部署中，我们会选择Webapi 或者 WebServie 等远程通信手段来进行交互  

遗憾的是，我们的应用是线下的，并发量也并不需要到集群这样重量级的解决方案，所以我们使用Abp的插件加载机制为基础设施, 
将每个上下文都实现成了一个个独立的项目模块.  

项目初期我们使用 Abp 提供的事件总线作为模块之间交互的方式, 但它有一个很不好的地方是, 它的事件引用必须是显式的原对象引用。    
这也就意味着，你为了在A模块中使用B模块发布的事件，你必须让两个上下文都引用这个事件对象，这显然加深了模块间的耦合。  

在参考了Abp, Medirator, NServerBus以及微软的示例项目 eShopOnContainers 我决定自己实现一个服务总线, 它要具有以下特点：
* 支持委托注册处理函数
* 支持 Req/Resp 模式
* 事件的接收与发布对象是非引用的(指你可以在不同模块间建立各自的事件类，只需要保证它们名称与结构相同即可)  

所以ServiceAnt出现了, ServiceAnt 的初期目标是一个进程内的消息中介者, 后期有时间会开发分布式的版本。

### 与其他类似框架的不同之处

这里只作一些不同之处的分析, 并不代表优劣, 请结合自己项目的情况来合理选择.

`Mediator`: Mediator 只支持Ioc来注册处理函数, 并且不支持委托注册. 另外它的定位是进程内使用的基础设施, 不适用于分布式系统, 在`eshopcontainer`中,它被作为单个微服务下实现 CQRS 的基础设施.

`NServerBus`: NServerBus 是一个偏重的框架, 而且它的定位就是解决分布式架构中的通信问题，并没有进程内的实现版本(它的Learning Transport可以看作进程内的实现，但官方并不推荐使用), 它更适用于分布式的复杂系统来说.

`Abp`: 在上面已经讨论过了, 另外它也不支持 Pub/Sub.如果你的项目不采用多模块的机制, 或者不介意模块间的相互引用, Abp自带的事件还是不错的.

`eShopOnContainers`: 这只是微软的示例项目，它其中的事件总线是分布式的，有两个实现，一个基于RabbitMQ一个基于AzureMQ, 它也没有作为框架发布到Nuget上.

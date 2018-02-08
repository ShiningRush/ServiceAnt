# ServiceAnt

[![Build status](https://ci.appveyor.com/api/projects/status/github/ShiningRush/serviceant?branch=master&svg=true)](https://ci.appveyor.com/project/ShiningRush/serviceant)
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/ShiningRush/ServiceAnt/blob/master/LICENSE)
[![NuGet](https://img.shields.io/nuget/vpre/serviceant.svg)](https://www.nuget.org/packages/ServiceAnt)

[中文介绍请点击这里](https://github.com/ShiningRush/ServiceAnt/blob/master/README.zh-cn.md)  

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

Before using Ioc registration, we need to integrate ServiceAnt into your Ioc environment first. Please refer to [Ioc Integration](#IocIntegration) to integrate ServiceAnt into your Ioc.  

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

But no matter how low the coupling, there will always be some high-level interaction, these are called "border points", usually in a distributed deployment, we will choose remote communication such as Webapi or WebServie to impletement it.  

Unfortunately, our application is offline, and concurrent traffic does not require such a heavyweight solution as clustering, so we implemented each context as an Project module using Abp's plug-in loading mechanism.   

Earlier in the project we used the event bus provided by Abp as a way to interact with the module, but one of the bad things about it is that its event reference must be an explicit reference to the original object.This means that you have to have both contexts reference this event object in order to use the events posted by the B module in module A, which obviously deepens the coupling between the modules.  

With reference to Abp, Medirator, NServerBus and Microsoft's example project eShopOnContainers, I decided to implement myself a service bus that has the following features：
* Supporting registering handler with delegate
* Supporting Req/Resp
* Trigger is not refferenced

ServiceAnt appeared, the initial goal of ServiceAnt is a message mediator within the process, late have time to develop a distributed version.

### Differences from other similar frameworks

Here only for some of the differences between the thoese, does not represent the merits, please choose a suitable one with your project.

`Mediator`: Mediator only supports Ioc to register handlers and does not support delegate registration. In addition, it is positioned as an in-process infrastructure and not for distributed systems. In eshoponcontainer, it is implemented as a CQRS for a single microservice infrastructure.

`NServerBus`: NServerBus is a biased framework, and its positioning is to solve the problem of communication in distributed architecture, there is no in-process implementation version (its Learning Transport can be seen as an in-process implementation, but the official is not recommended), it More suitable for distributed complex systems.

`Abp`: Discussed above, and it also does not support Pub / Sub. If your project does not use a multi-module mechanism, or do not mind the mutual reference between modules, Abp is a good choice.

`eShopOnContainers`: This is just an example project from Microsoft, where the event bus is distributed, with two implementations, one based on RabbitMQ and one based on AzureMQ, and it is also not published as a framework on Nuget.

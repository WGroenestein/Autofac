﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using NUnit.Framework;
using AutofacContrib.DynamicProxy2;
using Castle.Core.Interceptor;
using Autofac.Builder;

namespace AutofacContrib.Tests.DynamicProxy2
{
    [TestFixture]
    public class AttributedInterfaceInterceptionFixture
    {
        [Intercept(typeof(AddOneInterceptor))]
        public interface IHasI
        {
            int GetI();
        }

        public class C : IHasI
        {
            public int I { get; private set; }

            public C()
            {
                I = 10;
            }

            public int GetI()
            {
                return I;
            }
        }

        class AddOneInterceptor : IInterceptor
        {
            public void Intercept(IInvocation invocation)
            {
                invocation.Proceed();
                if (invocation.Method.Name == "GetI")
                    invocation.ReturnValue = 1 + (int)invocation.ReturnValue;
            }
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void DetectsNonInterfaceServices()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<C>().EnableInterfaceInterceptors();
            builder.RegisterType<AddOneInterceptor>();
            var c = builder.Build();
            c.Resolve<C>();
        }

        [Test]
        public void FindsInterceptionAttributeOnReflectionComponent()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<C>().As<IHasI>().EnableInterfaceInterceptors();
            builder.RegisterType<AddOneInterceptor>();
            var cpt = builder.Build().Resolve<IHasI>();

            Assert.AreEqual(11, cpt.GetI()); // proxied
        }

        [Test]
        public void FindsInterceptionAttributeOnExpressionComponent()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new C()).As<IHasI>().EnableInterfaceInterceptors();
            builder.RegisterType<AddOneInterceptor>();
            var cpt = builder.Build().Resolve<IHasI>();

            Assert.AreEqual(11, cpt.GetI()); // proxied
        }
    }
}
﻿// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using Autofac.Util;

namespace Autofac.Core.Activators.ProvidedInstance
{
    /// <summary>
    /// Provides a pre-constructed instance.
    /// </summary>
    public class ProvidedInstanceActivator : InstanceActivator, IInstanceActivator
    {
        readonly object _instance;
        bool _activated;
        bool _disposeInstance;

        /// <summary>
        /// Provide the specified instance.
        /// </summary>
        /// <param name="instance">The instance to provide.</param>
        public ProvidedInstanceActivator(object instance)
            : base(Enforce.ArgumentNotNull(instance, "instance").GetType())
        {
            _instance = instance;
        }

        /// <summary>
        /// Activate an instance in the provided context.
        /// </summary>
        /// <param name="context">Context in which to activate instances.</param>
        /// <param name="parameters">Parameters to the instance.</param>
        /// <returns>The activated instance.</returns>
        /// <remarks>
        /// The context parameter here should probably be ILifetimeScope in order to reveal Disposer,
        /// but will wait until implementing a concrete use case to make the decision
        /// </remarks>
        public object ActivateInstance(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (parameters == null) throw new ArgumentNullException("parameters");

            if (_activated)
                throw new InvalidOperationException(ProvidedInstanceActivatorResources.InstanceAlreadyActivated);

            _activated = true;

            return _instance;
        }

        /// <summary>
        /// Determines whether the activator disposes the instance that it holds.
        /// Necessary because otherwise instances that are never resolved will never be
        /// disposed.
        /// </summary>
        public bool DisposeInstance
        {
            get { return _disposeInstance; }
            set { _disposeInstance = value; }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            // Only dispose of the instance here if it wasn't activated. If it was activated,
            // then either the owning lifetime scope will dispose of it automatically
            // (see InstanceLookup.Activate) or an OnRelease handler will take care of it.
            if (disposing && _disposeInstance && _instance is IDisposable && !_activated)
                ((IDisposable)_instance).Dispose();

            base.Dispose(disposing);
        }
    }
}

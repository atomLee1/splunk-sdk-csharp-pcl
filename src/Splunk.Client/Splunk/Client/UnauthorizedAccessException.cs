﻿/*
 * Copyright 2014 Splunk, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"): you may
 * not use this file except in compliance with the License. You may obtain
 * a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 */

//// TODO:
//// [O] Contracts
//// [O] Documentation

namespace Splunk.Client
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Net.Http;

    /// <summary>
    /// The exception that is thrown when a request to access a resource results
    /// in <see cref="HttpStatusCode.Forbidden"/>.
    /// </summary>
    /// <seealso cref="T:Splunk.Client.RequestException"/>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification =
        "This is by design.")
    ]
    public sealed class UnauthorizedAccessException : RequestException
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="UnauthorizedAccessException"/>
        /// class.
        /// </summary>
        /// <param name="message">
        /// An object representing an HTTP response message including the status code
        /// and data.
        /// </param>
        /// <param name="details">
        /// A sequence of <see cref="Message"/> instances detailing the cause of the
        /// <see cref="UnauthorizedAccessException"/>.
        /// </param>
        internal UnauthorizedAccessException(HttpResponseMessage message, ReadOnlyCollection<Message> details)
            : base(message, details)
        {
            Contract.Requires<ArgumentException>(message.StatusCode == HttpStatusCode.Forbidden);
        }
    }
}

// ------------------------------------------------------------------------
// Copyright 2021 The Dapr Authors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Dapr.Client
{
    /// <summary>
    /// Represents the response from invoking a workflow.
    /// </summary>
    public sealed class WorkflowReference
    {
        /// <summary>
        /// Initializes a new <see cref="WorkflowReference" />.`
        /// </summary>
        /// <param name="instanceId">The instance ID assocated with this response.</param>
        public WorkflowReference(string instanceId)
        {
            ArgumentVerifier.ThrowIfNull(instanceId, nameof(instanceId));
            this.InstanceId = instanceId;
        }

        /// <summary>
        /// The instance ID assocated with this workflow.
        /// </summary>
        public string InstanceId { set; get; }

    }
}

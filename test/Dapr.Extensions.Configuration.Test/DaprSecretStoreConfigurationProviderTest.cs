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
using System.Net;
using System.Threading.Tasks;
using Dapr.Client;
using FluentAssertions;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Xunit;
using Autogenerated = Dapr.Client.Autogen.Grpc.v1;

namespace Dapr.Extensions.Configuration.Test
{
    // These tests use the outdated TestHttpClient infrastructure because they need to 
    // support testing with synchronous HTTP requests.
    //
    // Don't copy this pattern elsewhere.
    public class DaprSecretStoreConfigurationProviderTest
    {

        [Fact]
        public void AddDaprSecretStore_UsingDescriptors_WithoutStore_ReportsError()
        {
            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = new TestHttpClient() })
                .Build();

            var ex = Assert.Throws<ArgumentNullException>(() =>
                {
                    var config = CreateBuilder()
                        .AddDaprSecretStore(null, new DaprSecretDescriptor[] { new DaprSecretDescriptor("secretName") }, daprClient)
                        .Build();
                });

            Assert.Contains("store", ex.Message);
        }

        [Fact]
        public void AddDaprSecretStore_UsingDescriptors_WithEmptyStore_ReportsError()
        {
            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = new TestHttpClient() })
                .Build();

            var ex = Assert.Throws<ArgumentException>(() =>
                {
                    var config = CreateBuilder()
                        .AddDaprSecretStore(string.Empty, new DaprSecretDescriptor[] { new DaprSecretDescriptor("secretName") }, daprClient)
                        .Build();
                });

            Assert.Contains("The value cannot be null or empty", ex.Message);
        }

        [Fact]
        public void AddDaprSecretStore_UsingDescriptors_WithoutSecretDescriptors_ReportsError()
        {
            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = new TestHttpClient() })
                .Build();

            var ex = Assert.Throws<ArgumentNullException>(() =>
                {
                    var config = CreateBuilder()
                        .AddDaprSecretStore("store", (DaprSecretDescriptor[])null, daprClient)
                        .Build();
                });

            Assert.Contains("secretDescriptors", ex.Message);
        }

        [Fact]
        public void AddDaprSecretStore_UsingDescriptors_WithoutClient_ReportsError()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                {
                    var config = CreateBuilder()
                        .AddDaprSecretStore("store", new DaprSecretDescriptor[] { new DaprSecretDescriptor("secretName") }, null)
                        .Build();
                });

            Assert.Contains("client", ex.Message);
        }

        [Fact]
        public void AddDaprSecretStore_UsingDescriptors_WithZeroSecretDescriptors_ReportsError()
        {
            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = new TestHttpClient() })
                .Build();

            var ex = Assert.Throws<ArgumentException>(() =>
                {
                    var config = CreateBuilder()
                        .AddDaprSecretStore("store", new DaprSecretDescriptor[] { }, daprClient)
                        .Build();
                });

            Assert.Contains("No secret descriptor was provided", ex.Message);
        }

        [Fact]
        public void AddDaprSecretStore_UsingDescriptors_DuplicateSecret_ReportsError()
        {
            var httpClient = new TestHttpClient()
            {
                Handler = async (entry) =>
                   {
                       var secrets = new Dictionary<string, string>() { { "secretName", "secret" }, { "SecretName", "secret" } };
                       await SendResponseWithSecrets(secrets, entry);
                   }
            };

            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var ex = Assert.Throws<InvalidOperationException>(() =>
                {
                    var config = CreateBuilder()
                        .AddDaprSecretStore("store", new DaprSecretDescriptor[] { new DaprSecretDescriptor("secretName") }, daprClient)
                        .Build();
                });

            Assert.Contains("Please remove any duplicates from your secret store.", ex.Message);
        }

        [Fact]
        public void LoadSecrets_FromSecretStoreThatReturnsOneValue()
        {
            // Configure Client
            var httpClient = new TestHttpClient()
            {
                Handler = async (entry) =>
                   {
                       var secrets = new Dictionary<string, string>() { { "secretName", "secret" } };
                       await SendResponseWithSecrets(secrets, entry);
                   }
            };

            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var config = CreateBuilder()
                    .AddDaprSecretStore("store", new DaprSecretDescriptor[] { new DaprSecretDescriptor("secretName") }, daprClient)
                    .Build();

            config["secretName"].Should().Be("secret");
        }

        [Fact]
        public void LoadSecrets_FromSecretStoreThatCanReturnsMultipleValues()
        {
            // Configure Client
            var httpClient = new TestHttpClient()
            {
                Handler = async (entry) =>
                   {
                       var secrets = new Dictionary<string, string>() {
                           { "first_secret", "secret1" },
                           { "second_secret", "secret2" }};
                       await SendResponseWithSecrets(secrets, entry);
                   }
            };

            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var config = CreateBuilder()
                    .AddDaprSecretStore("store", new DaprSecretDescriptor[] { new DaprSecretDescriptor("secretName") }, daprClient)
                    .Build();

            config["first_secret"].Should().Be("secret1");
            config["second_secret"].Should().Be("secret2");
        }

        //Here
        [Fact]
        public void AddDaprSecretStore_WithoutStore_ReportsError()
        {
            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = new TestHttpClient() })
                .Build();

            var ex = Assert.Throws<ArgumentNullException>(() =>
                {
                    var config = CreateBuilder()
                        .AddDaprSecretStore(null, daprClient)
                        .Build();
                });

            Assert.Contains("store", ex.Message);
        }

        [Fact]
        public void AddDaprSecretStore_WithEmptyStore_ReportsError()
        {
            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = new TestHttpClient() })
                .Build();

            var ex = Assert.Throws<ArgumentException>(() =>
                {
                    var config = CreateBuilder()
                        .AddDaprSecretStore(string.Empty, daprClient)
                        .Build();
                });

            Assert.Contains("The value cannot be null or empty", ex.Message);
        }

        [Fact]
        public void AddDaprSecretStore_WithoutClient_ReportsError()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                {
                    var config = CreateBuilder()
                        .AddDaprSecretStore("store", null)
                        .Build();
                });

            Assert.Contains("client", ex.Message);
        }

        [Fact]
        public void AddDaprSecretStore_DuplicateSecret_ReportsError()
        {
            var httpClient = new TestHttpClient()
            {
                Handler = async (entry) =>
                   {
                       var secrets = new Dictionary<string, string>() { { "secretName", "secret" }, { "SecretName", "secret" } };
                       await SendBulkResponseWithSecrets(secrets, entry);
                   }
            };

            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var ex = Assert.Throws<InvalidOperationException>(() =>
                {
                    var config = CreateBuilder()
                        .AddDaprSecretStore("store", daprClient)
                        .Build();
                });

            Assert.Contains("Please remove any duplicates from your secret store.", ex.Message);
        }

        [Fact]
        public void BulkLoadSecrets_FromSecretStoreThatReturnsOneValue()
        {
            // Configure Client
            var httpClient = new TestHttpClient()
            {
                Handler = async (entry) =>
                   {
                       var secrets = new Dictionary<string, string>() { { "secretName", "secret" } };
                       await SendBulkResponseWithSecrets(secrets, entry);
                   }
            };

            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var config = CreateBuilder()
                    .AddDaprSecretStore("store", daprClient)
                    .Build();

            config["secretName"].Should().Be("secret");
        }

        [Fact]
        public void BulkLoadSecrets_FromSecretStoreThatCanReturnsMultipleValues()
        {
            // Configure Client
            var httpClient = new TestHttpClient()
            {
                Handler = async (entry) =>
                   {
                       var secrets = new Dictionary<string, string>() {
                           { "first_secret", "secret1" },
                           { "second_secret", "secret2" }};
                       await SendBulkResponseWithSecrets(secrets, entry);
                   }
            };

            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var config = CreateBuilder()
                    .AddDaprSecretStore("store", daprClient)
                    .Build();

            config["first_secret"].Should().Be("secret1");
            config["second_secret"].Should().Be("secret2");
        }

        [Fact]
        public void LoadSecrets_FromSecretStoreThatReturnsNonNormalizedKey()
        {
            // Configure Client
            var httpClient = new TestHttpClient()
            {
                Handler = async (entry) =>
                {
                    var secrets = new Dictionary<string, string>() { { "secretName__value", "secret" } };
                    await SendResponseWithSecrets(secrets, entry);
                }
            };

            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var config = CreateBuilder()
                    .AddDaprSecretStore("store", new DaprSecretDescriptor[] { new DaprSecretDescriptor("secretName__value") }, daprClient)
                    .Build();

            config["secretName:value"].Should().Be("secret");
        }

        [Fact]
        public void BulkLoadSecrets_FromSecretStoreThatReturnsNonNormalizedKey()
        {
            // Configure Client
            var httpClient = new TestHttpClient()
            {
                Handler = async (entry) =>
                {
                    var secrets = new Dictionary<string, string>() {
                           { "first_secret__value", "secret1" }};
                    await SendBulkResponseWithSecrets(secrets, entry);
                }
            };

            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var config = CreateBuilder()
                    .AddDaprSecretStore("store", daprClient)
                    .Build();

            config["first_secret:value"].Should().Be("secret1");
        }

        [Fact]
        public void LoadSecrets_FromSecretStoreThatReturnsNonNormalizedKeyDisabledNormalizeKey()
        {
            // Configure Client
            var httpClient = new TestHttpClient()
            {
                Handler = async (entry) =>
                {
                    var secrets = new Dictionary<string, string>() { { "secretName__value", "secret" } };
                    await SendResponseWithSecrets(secrets, entry);
                }
            };

            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var config = CreateBuilder()
                    .AddDaprSecretStore((conf) =>
                    {
                        conf.Store = "store";
                        conf.SecretDescriptors = new DaprSecretDescriptor[] { new DaprSecretDescriptor("secretName__value") };
                        conf.Client = daprClient;
                        conf.NormalizeKey = false;
                    })
                    .Build();

            config["secretName__value"].Should().Be("secret");
        }

        [Fact]
        public void BulkLoadSecrets_FromSecretStoreThatReturnsNonNormalizedKeyDisabledNormalizeKey()
        {
            // Configure Client
            var httpClient = new TestHttpClient()
            {
                Handler = async (entry) =>
                {
                    var secrets = new Dictionary<string, string>() {
                           { "first_secret__value", "secret1" }};
                    await SendBulkResponseWithSecrets(secrets, entry);
                }
            };

            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var config = CreateBuilder()
                    .AddDaprSecretStore((conf) =>
                    {
                        conf.Store = "store";
                        conf.Client = daprClient;
                        conf.NormalizeKey = false;
                    })
                    .Build();

            config["first_secret__value"].Should().Be("secret1");
        }

        [Fact]
        public void LoadSecrets_FromSecretStoreThatReturnsCustomDelimitedKey()
        {
            // Configure Client
            var httpClient = new TestHttpClient()
            {
                Handler = async (entry) =>
                {
                    var secrets = new Dictionary<string, string>()
                    {
                        ["secretName--value"] = "secret",
                    };
                    await SendResponseWithSecrets(secrets, entry);
                }
            };

            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var config = CreateBuilder()
                    .AddDaprSecretStore((conf) =>
                    {
                        conf.Store = "store";
                        conf.Client = daprClient;
                        conf.SecretDescriptors = new DaprSecretDescriptor[] { new DaprSecretDescriptor("secretName--value") };
                        conf.KeyDelimiters = new[] { "--" };
                    })
                    .Build();

            config["secretName:value"].Should().Be("secret");
        }

        [Fact]
        public void LoadSecrets_FromSecretStoreThatReturnsCustomDelimitedKey_NullDelimiters()
        {
            // Configure Client
            var httpClient = new TestHttpClient()
            {
                Handler = async (entry) =>
                {
                    var secrets = new Dictionary<string, string>()
                    {
                        ["secretName--value"] = "secret",
                    };
                    await SendResponseWithSecrets(secrets, entry);
                }
            };

            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var config = CreateBuilder()
                    .AddDaprSecretStore((conf) =>
                    {
                        conf.Store = "store";
                        conf.Client = daprClient;
                        conf.SecretDescriptors = new DaprSecretDescriptor[] { new DaprSecretDescriptor("secretName--value") };
                        conf.KeyDelimiters = null;
                    })
                    .Build();

            config["secretName--value"].Should().Be("secret");
        }

        [Fact]
        public void LoadSecrets_FromSecretStoreThatReturnsCustomDelimitedKey_EmptyDelimiters()
        {
            // Configure Client
            var httpClient = new TestHttpClient()
            {
                Handler = async (entry) =>
                {
                    var secrets = new Dictionary<string, string>()
                    {
                        ["secretName--value"] = "secret",
                    };
                    await SendResponseWithSecrets(secrets, entry);
                }
            };

            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var config = CreateBuilder()
                    .AddDaprSecretStore((conf) =>
                    {
                        conf.Store = "store";
                        conf.Client = daprClient;
                        conf.SecretDescriptors = new DaprSecretDescriptor[] { new DaprSecretDescriptor("secretName--value") };
                        conf.KeyDelimiters = new List<string>();
                    })
                    .Build();

            config["secretName--value"].Should().Be("secret");
        }

        [Fact]
        public void LoadSecrets_FromSecretStoreThatReturnsDifferentCustomDelimitedKeys()
        {
            // Configure Client
            var httpClient = new TestHttpClient()
            {
                Handler = async (entry) =>
                {
                    // The following is an attempt at handling multiple secret descriptors for unit tests.
                    var content = await entry.Request.Content.ReadAsStringAsync();
                    if (content.Contains("secretName--value"))
                    {
                        await SendResponseWithSecrets(new Dictionary<string, string>()
                        {
                            ["secretName--value"] = "secret",
                        }, entry);
                    }
                    else if (content.Contains("otherSecretName≡value"))
                    {
                        await SendResponseWithSecrets(new Dictionary<string, string>()
                        {
                            ["otherSecretName≡value"] = "secret",
                        }, entry);
                    }
                }
            };

            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var config = CreateBuilder()
                    .AddDaprSecretStore((conf) =>
                    {
                        conf.Store = "store";
                        conf.Client = daprClient;
                        conf.SecretDescriptors = new DaprSecretDescriptor[]
                        {
                            new DaprSecretDescriptor("secretName--value"),
                            new DaprSecretDescriptor("otherSecretName≡value"),
                        };
                        conf.KeyDelimiters = new[] { "--", "≡" };
                    })
                    .Build();

            config["secretName:value"].Should().Be("secret");
            config["otherSecretName:value"].Should().Be("secret");
        }

        [Fact]
        public void BulkLoadSecrets_FromSecretStoreThatReturnsCustomDelimitedKey()
        {
            // Configure Client
            var httpClient = new TestHttpClient()
            {
                Handler = async (entry) =>
                {
                    var secrets = new Dictionary<string, string>()
                    {
                        ["first_secret--value"] = "secret1"
                    };
                    await SendBulkResponseWithSecrets(secrets, entry);
                }
            };

            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var config = CreateBuilder()
                    .AddDaprSecretStore("store", daprClient, new[] { "--" })
                    .Build();

            config["first_secret:value"].Should().Be("secret1");
        }

        [Fact]
        public void BulkLoadSecrets_FromSecretStoreThatReturnsDifferentCustomDelimitedKey()
        {
            // Configure Client
            var httpClient = new TestHttpClient()
            {
                Handler = async (entry) =>
                {
                    var secrets = new Dictionary<string, string>()
                    {
                        ["first_secret--value"] = "secret1",
                        ["second_secret≡value"] = "secret2",
                    };
                    await SendBulkResponseWithSecrets(secrets, entry);
                }
            };

            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var config = CreateBuilder()
                    .AddDaprSecretStore("store", daprClient, new[] { "--", "≡" })
                    .Build();

            config["first_secret:value"].Should().Be("secret1");
            config["second_secret:value"].Should().Be("secret2");
        }

        [Fact]
        public void BulkLoadSecrets_FromSecretStoreThatReturnsPlainAndCustomDelimitedKey()
        {
            // Configure Client
            var httpClient = new TestHttpClient()
            {
                Handler = async (entry) =>
                {
                    var secrets = new Dictionary<string, string>()
                    {
                        ["first_secret:value"] = "secret1",
                        ["second_secret--value"] = "secret2",
                        ["third_secret≡value"] = "secret3",
                    };
                    await SendBulkResponseWithSecrets(secrets, entry);
                }
            };

            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var config = CreateBuilder()
                    .AddDaprSecretStore("store", daprClient, new[] { "--", "≡" })
                    .Build();

            config["first_secret:value"].Should().Be("secret1");
            config["second_secret:value"].Should().Be("secret2");
            config["third_secret:value"].Should().Be("secret3");
        }

        [Fact]
        public void LoadSecrets_FromSecretStoreThatReturnsCustomDelimitedKeyDisabledNormalizeKey()
        {
            // Configure Client
            var httpClient = new TestHttpClient()
            {
                Handler = async (entry) =>
                {
                    var secrets = new Dictionary<string, string>()
                    {
                        ["secretName--value"] = "secret"
                    };
                    await SendResponseWithSecrets(secrets, entry);
                }
            };

            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var config = CreateBuilder()
                    .AddDaprSecretStore((conf) =>
                    {
                        conf.Store = "store";
                        conf.Client = daprClient;
                        conf.SecretDescriptors = new DaprSecretDescriptor[] { new DaprSecretDescriptor("secretName--value") };
                        conf.NormalizeKey = false;
                        conf.KeyDelimiters = new[] { "--" };
                    })
                    .Build();

            config["secretName--value"].Should().Be("secret");
        }

        [Fact]
        public void BulkLoadSecrets_FromSecretStoreThatReturnsCustomDelimitedKeyDisabledNormalizeKey()
        {
            // Configure Client
            var httpClient = new TestHttpClient()
            {
                Handler = async (entry) =>
                {
                    var secrets = new Dictionary<string, string>()
                    {
                        ["first_secret--value"] = "secret1"
                    };
                    await SendBulkResponseWithSecrets(secrets, entry);
                }
            };

            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var config = CreateBuilder()
                    .AddDaprSecretStore((conf) =>
                    {
                        conf.Store = "store";
                        conf.Client = daprClient;
                        conf.NormalizeKey = false;
                        conf.KeyDelimiters = new[] { "--" };
                    })
                    .Build();

            config["first_secret--value"].Should().Be("secret1");
        }

        private IConfigurationBuilder CreateBuilder()
        {
            return new ConfigurationBuilder();
        }

        private async Task SendResponseWithSecrets(Dictionary<string, string> secrets, TestHttpClient.Entry entry)
        {
            var secretResponse = new Autogenerated.GetSecretResponse();
            secretResponse.Data.Add(secrets);

            var streamContent = await GrpcUtils.CreateResponseContent(secretResponse);
            var response = GrpcUtils.CreateResponse(HttpStatusCode.OK, streamContent);
            entry.Completion.SetResult(response);
        }

        private async Task SendBulkResponseWithSecrets(Dictionary<string, string> secrets, TestHttpClient.Entry entry)
        {
            var getBulkSecretResponse = new Autogenerated.GetBulkSecretResponse();
            foreach (var secret in secrets)
            {
                var secretsResponse = new Autogenerated.SecretResponse();
                secretsResponse.Secrets[secret.Key] = secret.Value;
                // Bulk secret response is `MapField<string, MapField<string, string>>`. The outer key (string) must be ignored by `DaprSecretStoreConfigurationProvider`.
                getBulkSecretResponse.Data.Add("IgnoredKey" + secret.Key, secretsResponse);
            }

            var streamContent = await GrpcUtils.CreateResponseContent(getBulkSecretResponse);
            var response = GrpcUtils.CreateResponse(HttpStatusCode.OK, streamContent);
            entry.Completion.SetResult(response);
        }
    }
}

using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure; 

var app = new App();
new InfrastructureStack(app, "InfrastructureStack", new StackProps
{
    // If you don't specify 'env', this stack will be environment-agnostic.
    // Account/Region-dependent features and context lookups will not work,
    // but a single synthesized template can be deployed anywhere.

    // Uncomment the next block to specialize this stack for the AWS Account
    // and Region that are implied by the current CLI configuration.
    /*
    Env = new Amazon.CDK.Environment
    {
        Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
        Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION"),
    }
    */

    // Uncomment the next block if you know exactly what Account and Region you
    // want to deploy the stack to.
    /**/
    Env = new Amazon.CDK.Environment
    {
        Account = "425173371283",
        Region = "us-east-2",
    }
});
app.Synth();

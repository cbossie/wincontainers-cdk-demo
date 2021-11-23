using System.Security.Cryptography;
using System.Linq;
using System.Reflection.Metadata;
using System.IO;
using Amazon.CDK;
using Amazon.CDK.RegionInfo;
using Constructs;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.DirectoryService;
using Amazon.CDK.AWS.SSM;
using Amazon.CDK.AWS.SecretsManager;
using Amazon.CDK.AWS.FSx;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.ECS;
using static Amazon.CDK.AWS.DirectoryService.CfnMicrosoftAD;

namespace Infrastructure
{
    public class InfrastructureStack : Stack
    {

        internal InfrastructureStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {

            // Part 1 - Network
            var vpc = new Vpc(this, "StackVpc", new VpcProps{
                Cidr = "10.0.0.0/16",
                MaxAzs = 3,
                NatGateways = 2
            });

            // Part 2 - Domain Password
            var gen = new Amazon.CDK.AWS.SecretsManager.SecretStringGenerator{
                PasswordLength = 10
            };
            var secret = new Secret(this, "adpassword", new SecretProps{
                Description = "Active Directory Password",
                SecretName = "adpassword",
                GenerateSecretString = gen
            });


            // Part 3 - Domain



            var dir = new CfnMicrosoftAD(this, "StackAD", new CfnMicrosoftADProps{
                Edition = "Standard",
                Name = "wincontainer.local",
                Password = secret.SecretValue.ToString(),
                VpcSettings = new VpcSettingsProperty{
                    VpcId = vpc.VpcId,
                    SubnetIds = vpc.PrivateSubnets.Take(2).Select(a => a.SubnetId).ToArray()
                },
                ShortName = "wincontainer"
            });

            // Part 4 - FSX
            var fsx = new Amazon.CDK.AWS.FSx.CfnFileSystem(this, "fileSystem", new CfnFileSystemProps{
                FileSystemType = "WINDOWS",
                StorageCapacity = 100,
                StorageType = "SSD",
                SubnetIds = vpc.PrivateSubnets.Take(2).Select(a => a.SubnetId).ToArray(),
                WindowsConfiguration = new CfnFileSystem.WindowsConfigurationProperty {
                    ActiveDirectoryId = Amazon.CDK.Fn.Ref(dir.LogicalId),
                    DeploymentType = "MULTI_AZ_1",
                    PreferredSubnetId = vpc.PrivateSubnets.First().SubnetId,
                    ThroughputCapacity = 8,
                },
            });

            // Part 5 ECR Repository
            var repo = new Repository(this, "apprepo", new RepositoryProps{
                RepositoryName = "containerizedapp",
                RemovalPolicy = RemovalPolicy.RETAIN
            });
            new CfnOutput(this, "repo", new CfnOutputProps{
                ExportName = "repooutput",
                Description = "Application Container Repository URI",
                Value = repo.RepositoryUri
            });

            // // Part 6 - ECS Cluster
            // var cluster = new Cluster(this, "wincontainercluster", new ClusterProps{
            //     Capacity = new AddCapacityOptions{
            //         DesiredCapacity = 3,
            //         VpcSubnets = new SubnetSelection
            //         {
            //             Subnets = 
            //         }
            //     }
            // });


        }
    }
}

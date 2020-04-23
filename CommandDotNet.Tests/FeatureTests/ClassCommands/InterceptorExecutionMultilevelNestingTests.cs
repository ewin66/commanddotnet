using System.Threading.Tasks;
using CommandDotNet.Tests.Utils;
using CommandDotNet.TestTools;
using CommandDotNet.TestTools.Scenarios;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CommandDotNet.Tests.FeatureTests.ClassCommands
{
    public class InterceptorExecutionMultilevelNestingTests
    {
        public InterceptorExecutionMultilevelNestingTests(ITestOutputHelper output)
        {
            Ambient.Output = output;
        }

        [Fact]
        public void WhenChildCommandsAreNotRequested_TheirInterceptorsAreNotExecuted()
        {
            new AppRunner<Level1>()
                .InjectTrackingInvocations()
                .Verify(new Scenario
                {
                    When = {Args = "--name lala Do"},
                    Then =
                    {
                        AssertContext = ctx =>
                        {
                            ctx.GetInterceptorInvocation<Level1, TrackingInvocation>().WasInvoked.Should().BeTrue();
                            ctx.GetInterceptorInvocation<Level2, TrackingInvocation>().Should().BeNull();
                            ctx.GetInterceptorInvocation<Level3, TrackingInvocation>().Should().BeNull();
                        }
                    }
                });
        }

        [Fact]
        public void WhenChildCommandsAreRequested_AllAncestorInterceptorsAreExecuted()
        {
            // this test also proves we can NOT use the same option name for each command because they will conflict.
            // TODO: allow same name. Requires update to how ArgumentValues are keyed.
            new AppRunner<Level1>()
                .InjectTrackingInvocations()
                .Verify(new Scenario
                {
                    When = { Args = "--name lala Level2 --name2 lala Level3 --name3 fishies Do" },
                    Then =
                    {
                        AssertContext = ctx =>
                        {
                            ctx.GetInterceptorInvocation<Level1, TrackingInvocation>().WasInvoked.Should().BeTrue();
                            ctx.GetInterceptorInvocation<Level2, TrackingInvocation>().WasInvoked.Should().BeTrue();
                            ctx.GetInterceptorInvocation<Level3, TrackingInvocation>().WasInvoked.Should().BeTrue();
                        }
                    }
                });
        }

        [Fact]
        public void InterceptorsAreNotExecutedWhenTheirCommandIsNotInTheRequestPipeline()
        {
            // this test also proves we can NOT use the same option name for each command because they will conflict.
            // TODO: allow same name. Requires update to how ArgumentValues are keyed.
            new AppRunner<Level1>()
                .InjectTrackingInvocations()
                .Verify(new Scenario
                {
                    When = {Args = "--name lala Level3 --name3 fishies Do"},
                    Then =
                    {
                        AssertContext = ctx =>
                        {
                            ctx.GetInterceptorInvocation<Level1, TrackingInvocation>().WasInvoked.Should().BeTrue();
                            ctx.GetInterceptorInvocation<Level2, TrackingInvocation>().Should().BeNull();
                            ctx.GetInterceptorInvocation<Level3, TrackingInvocation>().WasInvoked.Should().BeTrue();
                        }
                    }
                });
        }

        class Level1
        {
            [SubCommand]
            public Level2 Level2 { get; set; }
            [SubCommand]
            public Level3 Level3 { get; set; }

            public string Name { get; private set; }

            public Task<int> Intercept(InterceptorExecutionDelegate next, string name)
            {
                Name = name;
                if (Level2 != null)
                {
                    // will be null if GrandChild is called directly or this.Do is called
                    Level2.ParentName = name;
                }
                if (Level3 != null)
                {
                    // will be null if GrandChild is NOT called directly or this.Do is called
                    Level3.GrandParentName = name;
                }
                return next();
            }

            public void Do() { }
        }

        class Level2
        {
            [SubCommand]
            public Level3 MyChild { get; set; }

            public string ParentName { get; set; }
            public string Name { get; private set; }

            public Task<int> Intercept(InterceptorExecutionDelegate next, string name2)
            {
                Name = name2;
                if (MyChild != null)
                {
                    // will be null if this.Do is called
                    MyChild.ParentName = name2;
                    MyChild.GrandParentName = ParentName;
                }
                return next();
            }

            public void Do() {}
        }

        class Level3
        {
            public string GrandParentName { get; set; }
            public string ParentName { get; set; }
            public string Name { get; private set; }

            public Task<int> Intercept(InterceptorExecutionDelegate next, string name3)
            {
                Name = name3;
                return next();
            }
            
            public void Do() {}
        }
    }
}

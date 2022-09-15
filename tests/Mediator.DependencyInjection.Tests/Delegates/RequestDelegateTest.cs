using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zapto.Mediator;

namespace Mediator.DependencyInjection.Tests.Delegates;

public record StringRequest : IRequest<string>;

public record LongRequest : IRequest<long>;

public class RequestDelegateTest
{
    [Fact]
    public async Task Valid()
    {
        const string expected = "success";

        await using var provider = new ServiceCollection()
            .AddMediator(b => b.AddRequestHandler((StringRequest _) => expected))
            .BuildServiceProvider();

        Assert.Equal(
            expected,
            await provider.GetRequiredService<IMediator>().Send<StringRequest, string>(new StringRequest())
        );
    }

    [Fact]
    public async Task ValidTask()
    {
        const string expected = "success";

        await using var provider = new ServiceCollection()
            .AddMediator(b => b.AddRequestHandler((StringRequest _) => Task.FromResult(expected)))
            .BuildServiceProvider();

        Assert.Equal(
            expected,
            await provider.GetRequiredService<IMediator>().Send<StringRequest, string>(new StringRequest())
        );
    }

    [Fact]
    public async Task ValidValueTask()
    {
        const string expected = "success";

        await using var provider = new ServiceCollection()
            .AddMediator(b => b.AddRequestHandler((StringRequest _) => new ValueTask<string>(expected)))
            .BuildServiceProvider();

        Assert.Equal(
            expected,
            await provider.GetRequiredService<IMediator>().Send<StringRequest, string>(new StringRequest())
        );
    }

    [Fact]
    public async Task ValidCast()
    {
        await using var provider = new ServiceCollection()
            .AddMediator(b => b.AddRequestHandler((LongRequest _) => 1))
            .BuildServiceProvider();

        Assert.Equal(
            1L,
            await provider.GetRequiredService<IMediator>().Send<LongRequest, long>(new LongRequest())
        );
    }

    [Fact]
    public async Task ValidTaskCast()
    {
        await using var provider = new ServiceCollection()
            .AddMediator(b => b.AddRequestHandler((LongRequest _) => Task.FromResult(1)))
            .BuildServiceProvider();

        Assert.Equal(
            1L,
            await provider.GetRequiredService<IMediator>().Send<LongRequest, long>(new LongRequest())
        );
    }

    [Fact]
    public async Task ValidValueTaskCast()
    {
        await using var provider = new ServiceCollection()
            .AddMediator(b => b.AddRequestHandler((LongRequest _) => new ValueTask<int>(1)))
            .BuildServiceProvider();

        Assert.Equal(
            1L,
            await provider.GetRequiredService<IMediator>().Send<LongRequest, long>(new LongRequest())
        );
    }

    [Fact]
    public void InvalidReturnType()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            new ServiceCollection()
                .AddMediator()
                .AddRequestHandler((StringRequest _) => 1);
        });
    }

    [Fact]
    public void InvalidReturnTaskType()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            new ServiceCollection()
                .AddMediator()
                .AddRequestHandler((StringRequest _) => Task.FromResult(1));
        });
    }

    [Fact]
    public void InvalidReturnValueTaskType()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            new ServiceCollection()
                .AddMediator()
                .AddRequestHandler((StringRequest _) => new ValueTask<int>(1));
        });
    }

    [Fact]
    public void NoRequest()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            new ServiceCollection()
                .AddMediator()
                .AddRequestHandler(() => "");
        });
    }

    [Fact]
    public void MultipleRequests()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            new ServiceCollection()
                .AddMediator()
                .AddRequestHandler((StringRequest _, StringRequest _) => "");
        });
    }
}

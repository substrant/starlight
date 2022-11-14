using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Starlight.Apis.JoinGame;
using Starlight.Misc;

// ReSharper disable once CheckNamespace
namespace Starlight.Apis;

public partial class Page<T>
{
    /// <summary>Synchronous wrapper for <see cref="FetchAsync"/>.</summary>
    public T[] Fetch(int newPageNumber)
    {
        return AsyncHelpers.RunSync(() => FetchAsync(newPageNumber));
    }

    /// <summary>Synchronous wrapper for <see cref="FetchNextAsync"/>.</summary>
    public T[] FetchNext()
    {
        return AsyncHelpers.RunSync(() => FetchNextAsync());
    }
    
    /// <summary>Synchronous wrapper for <see cref="FetchNextAsync"/>.</summary>
    public T[] FetchPrevious()
    {
        return AsyncHelpers.RunSync(() => FetchPreviousAsync());
    }
}
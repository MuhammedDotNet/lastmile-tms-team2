using FluentAssertions;
using LastMile.TMS.Domain.Entities;

namespace LastMile.TMS.Domain.Tests;

public class RouteTests
{
    private static readonly DateTimeOffset T0 = new(2025, 3, 1, 8, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset T1 = new(2025, 3, 1, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset T2 = new(2025, 3, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset T3 = new(2025, 3, 1, 14, 0, 0, TimeSpan.Zero);

    [Fact]
    public void TimeRangesOverlap_WhenIntervalsOverlap_ReturnsTrue()
    {
        Route.TimeRangesOverlap(T1, T2, T1, T3).Should().BeTrue();
    }

    [Fact]
    public void TimeRangesOverlap_WhenTouchingAtBoundary_ReturnsFalse()
    {
        Route.TimeRangesOverlap(T1, T2, T2, T3).Should().BeFalse();
    }

    [Fact]
    public void TimeRangesOverlap_WhenDisjoint_ReturnsFalse()
    {
        Route.TimeRangesOverlap(T0, T1, T2, T3).Should().BeFalse();
    }

    [Fact]
    public void TimeRangesOverlap_OpenEndedNewVersusBoundedExisting_OverlapsWhenNewStartsBeforeExistingEnds()
    {
        // New [10:00, ∞) vs existing [08:00, 12:00)
        Route.TimeRangesOverlap(T1, null, T0, T2).Should().BeTrue();
    }

    [Fact]
    public void TimeRangesOverlap_OpenEndedNewVersusBoundedExisting_NoOverlapWhenNewStartsAtOrAfterExistingEnd()
    {
        Route.TimeRangesOverlap(T3, null, T1, T2).Should().BeFalse();
    }

    [Fact]
    public void TimeRangesOverlap_BothOpenEndedOverlap()
    {
        Route.TimeRangesOverlap(T1, null, T0, null).Should().BeTrue();
    }

    /// <summary>
    /// CreateRouteCommand uses an EF-translatable form; it must stay equivalent to
    /// <see cref="Route.TimeRangesOverlap"/> when the new route has no end (CreateRouteDto has only StartDate).
    /// </summary>
    [Fact]
    public void UnboundedNewRoute_HandlerPredicate_MatchesTimeRangesOverlap()
    {
        var requestedStart = T1;
        var requestedEndExclusive = DateTimeOffset.MaxValue;

        var cases = new (DateTimeOffset RStart, DateTimeOffset? REnd)[]
        {
            (T0, T2),
            (T2, T3),
            (T0, T1),
            (T1, T2),
            (T3, null),
            (T0, null),
        };

        foreach (var (rStart, rEnd) in cases)
        {
            var handlerPredicate =
                rStart < requestedEndExclusive
                && requestedStart < (rEnd ?? DateTimeOffset.MaxValue);

            handlerPredicate.Should().Be(Route.TimeRangesOverlap(requestedStart, null, rStart, rEnd));
        }
    }
}

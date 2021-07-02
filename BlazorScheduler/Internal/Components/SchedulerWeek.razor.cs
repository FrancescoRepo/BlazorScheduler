﻿using BlazorScheduler.Internal.Extensions;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlazorScheduler.Internal.Components
{
	public partial class SchedulerWeek<T> where T : IAppointment, new()
    {
        [CascadingParameter] public Scheduler<T> Scheduler { get; set; }
        
        [Parameter] public DateTime Start { get; set; }
        [Parameter] public DateTime End { get; set; }
        [Parameter] public IEnumerable<T> Appointments { get; set; }

		private readonly Dictionary<T, int> Orderings = new();
		private readonly int MaxNumOfAppointmentsPerDay = 5;

        protected override void OnParametersSet()
        {
            Orderings.Clear();
            foreach (var app in Appointments)
            {
                Orderings[app] = GetBestOrderingForAppointment(app);
            }

            base.OnParametersSet();
        }

        private (int, int) GetStartAndEndDayForAppointment(T appointment)
        {
            DayOfWeek start = DayOfWeek.Sunday, end = DayOfWeek.Saturday;

            if (appointment.Start.Between(Start, End))
            {
                start = appointment.Start.DayOfWeek;
                end = appointment.End.Between(Start, End) ? appointment.End.DayOfWeek : DayOfWeek.Saturday;
            }
            else if (appointment.End.Between(Start, End))
            {
                start = DayOfWeek.Sunday;
                end = appointment.End.DayOfWeek;
            }

            return ((int)start, (int)end);
        }

        private int GetBestOrderingForAppointment(T appointment)
        {
            if (ReferenceEquals(appointment, Scheduler.NewAppointment))
            {
                return -1;
            }

            return Orderings
                .Where(x => {
                    return GetStartAndEndDayForAppointment(appointment).Overlaps(GetStartAndEndDayForAppointment(x.Key))
                        && !ReferenceEquals(x.Key, Scheduler.NewAppointment);
                })
                .OrderBy(x => x.Value)
                .TakeWhile((x, i) => x.Value == ++i)
                .LastOrDefault().Value + 1;
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOldOneLoginProcessEvents : Migration
    {
        /// <inheritdoc />
       protected override void Up(MigrationBuilder migrationBuilder) 
        {
            migrationBuilder.Sql(
                """
               /** Backup existing data for OneLoginEvents **/
               CREATE TABLE public.process_events_backup_onelogin AS
               SELECT *
               FROM public.process_events
               WHERE event_name IN (
                   'OneLoginUserCreatedEvent',
                   'OneLoginUserUpdatedEvent'
               );
               
               WITH previous_events AS (
                   SELECT
                       current_evt.process_event_id,
                       prev_evt.payload -> 'OneLoginUser' AS previous_onelogin_user
                   FROM public.process_events current_evt
               
                   CROSS JOIN LATERAL (
                       SELECT pe.*
                       FROM public.process_events pe
                       WHERE pe.event_name IN (
                               'OneLoginUserCreatedEvent',
                               'OneLoginUserUpdatedEvent'
                             )
               
                         -- both events must have a OneLogin subject
                         AND cardinality(pe.one_login_user_subjects) > 0
                         AND cardinality(current_evt.one_login_user_subjects) > 0
               
                         -- same OneLogin subject
                         AND pe.one_login_user_subjects[1]
                             = current_evt.one_login_user_subjects[1]
               
                         -- older event only
                         AND pe.created_on < current_evt.created_on
               
                       ORDER BY
                           pe.created_on DESC,
                           pe.process_event_id DESC
               
                       LIMIT 1
                   ) prev_evt
               
                   WHERE current_evt.event_name = 'OneLoginUserUpdatedEvent'
               )
               
               /** update OldOneLoginUser process_events **/
               UPDATE public.process_events current_evt
               SET payload = jsonb_set(
                   current_evt.payload,
                   '{OldOneLoginUser}',
                   previous_events.previous_onelogin_user,
                   true
               )
               FROM previous_events
               WHERE previous_events.process_event_id = current_evt.process_event_id
               
                 AND (
                     current_evt.payload -> 'OldOneLoginUser'
                 ) IS DISTINCT FROM (
                     previous_events.previous_onelogin_user
                 );
               """
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP TABLE IF EXISTS public.process_events_backup_onelogin;
                """
            );
        }

    }
}
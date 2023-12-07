using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class SourceUserIdEventSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                update events set
                payload = jsonb_set(payload, array['SourceUserId'], to_jsonb(payload->>'ActivatedByUserId'::text), true) - 'ActivatedByUserId'
                where event_name = 'UserActivatedEvent'
                and payload->>'SourceUserId' is null;

                update events set
                payload = jsonb_set(payload, array['SourceUserId'], to_jsonb(payload->>'AddedByUserId'::text), true) - 'AddedByUserId'
                where event_name = 'UserAddedEvent'
                and payload->>'SourceUserId' is null;

                update events set
                payload = jsonb_set(payload, array['SourceUserId'], to_jsonb(payload->>'DeactivatedByUserId'::text), true) - 'DeactivatedByUserId' - 'Changes'
                where event_name = 'UserDeactivatedEvent'
                and payload->>'SourceUserId' is null;
                
                update events set
                payload = jsonb_set(payload, array['SourceUserId'], to_jsonb(payload->>'UpdatedByUserId'::text), true) - 'UpdatedByUserId'
                where event_name = 'UserUpdatedEvent'
                and payload->>'SourceUserId' is null;

                update events set
                payload = jsonb_set(payload, array['SourceUserId'], to_jsonb('a81394d1-a498-46d8-af3e-e077596ab303'::text), true)
                where event_name in ('EytsAwardedEmailSentEvent', 'QtsAwardedEmailSentEvent', 'InductionCompletedEmailSentEvent', 'InternationalQtsAwardedEmailSentEvent')
                and payload->>'SourceUserId' is null;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                update events set
                payload = payload - 'SourceUserId'
                where event_name in ('EytsAwardedEmailSentEvent', 'QtsAwardedEmailSentEvent', 'InductionCompletedEmailSentEvent', 'InternationalQtsAwardedEmailSentEvent')
                and payload->>'SourceUserId' is not null;

                update events set
                payload = (payload || jsonb_build_object('UpdatedByUserId', payload->>'SourceUserId')) - 'SourceUserId'
                where event_name = 'UserUpdatedEvent'
                and payload->>'SourceUserId' is not null;

                update events set
                payload = (payload || jsonb_build_object('Changes', 0) || jsonb_build_object('DeactivatedByUserId', payload->>'SourceUserId')) - 'SourceUserId'
                where event_name = 'UserDeactivatedEvent'
                and payload->>'SourceUserId' is not null;
                
                update events set
                payload = (payload || jsonb_build_object('AddedByUserId', payload->>'SourceUserId')) - 'SourceUserId'
                where event_name = 'UserAddedEvent'
                and payload->>'SourceUserId' is not null;

                update events set
                payload = (payload || jsonb_build_object('ActivatedByUserId', payload->>'SourceUserId')) - 'SourceUserId'
                where event_name = 'UserActivatedEvent'
                and payload->>'SourceUserId' is not null;
                """);
        }
    }
}

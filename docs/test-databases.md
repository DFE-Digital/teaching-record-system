# Test databases

Tests run against Postgres in a test container. There's nothing to configure beyond having Docker running.

## How it works

- **One container per machine.** `trs-tests-postgres-17` is shared by every test process, in every git worktree. It's started by the first process that needs it and left running, so later runs start quickly. It listens on a random host port.
- **Nothing outlives the container.** Its data lives on tmpfs and it's removed when it stops, so there are no volumes or stopped containers to clean up. Restarting Docker just means the next run rebuilds the template.
- **A template per build.** The template is migrated and seeded once, and named after the versions of the `TeachingRecordSystem.Core` and `TeachingRecordSystem.TestCommon` assemblies, since migrations and seed data live in those. Changing either builds a new template, which takes a couple of seconds. The newest five are kept.
- **A database per process.** Each test process takes the first free `trs_test_<n>` slot and clones the template into it, so every run starts clean. It holds an advisory lock on the slot until it exits, including if it crashes.

Because a process's database is exclusively its own, test projects and worktrees can run at the same time without affecting each other.

Tests inside a process still share that process's database. Most run in parallel, so they mustn't assume they're the only ones writing. Tests that need an empty database use `[ClearDbBeforeTest]` in a `DisableParallelization` collection.

## Inspecting a database

A slot's database stays in place until another process takes that slot, so it can be examined after a run:

```shell
docker exec -it trs-tests-postgres-17 psql -U postgres -l
docker exec -it trs-tests-postgres-17 psql -U postgres -d trs_test_1
```

## Starting again

Stop the container; the next run creates a new one:

```shell
docker stop trs-tests-postgres-17
```

Do this after changing the container's settings in `TestDatabase`, too. A running container with the old settings otherwise keeps its name, and new runs fail to start.

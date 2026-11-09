# Test databases

Tests run against Postgres in a test container. There's nothing to configure beyond having Docker running.

## How it works

- **One container per machine.** `trs-tests-postgres-17` is shared by every test process, in every git worktree. It's started by the first process that needs it and left running, so later runs start quickly. It listens on a random host port.
- **Nothing outlives the container.** Its data lives on tmpfs and it's removed when it stops, so there are no volumes or stopped containers to clean up. Restarting Docker just means the next run rebuilds the template.
- **A template per build.** The template is migrated and seeded once, and named after the versions of the `TeachingRecordSystem.Core` and `TeachingRecordSystem.TestCommon` assemblies, since migrations and seed data live in those. Changing either builds a new template, which takes a couple of seconds. The newest five are kept.
- **A database per process.** Each test process takes the first free `trs_test_<n>` slot and clones the template into it, so every run starts clean. It holds an advisory lock on the slot until it exits, including if it crashes.

Because a process's database is exclusively its own, test projects and worktrees can run at the same time without affecting each other.

Tests inside a process still share that process's database. Most run in parallel, so they mustn't assume they're the only ones writing. Tests that need an empty database use `[ClearDbBeforeTest]` in a `DisableParallelization` collection.

## Running serial tests side by side

Tests in a `DisableParallelization` collection run one at a time within a process, so `scripts/RunTests.cs` splits a project's classes across several processes, each with its own database. CI does this for every project except `EndToEndTests`, whose hosts listen on fixed ports; that project gets more jobs instead. To do the same locally:

```shell
just test-project Core.Tests
```

Classes are balanced using `tests/test-timings.json`, which also decides how many jobs CI gives a project. A class with no timing yet is estimated from its number of tests, so the file only affects how even the split is. To refresh it from a CI run that tested every project:

```shell
just update-test-timings <run ID>
```

Be careful with this: tests run slower when processes share a runner, so timings from a run that used several processes per job are about 3–4 times higher for the larger projects, and CI would give those projects too many jobs. Lower the shard count calculation in `pr.yml` to match if you refresh them.

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

### Docker Images

- As few unique configurations as possible
- Can publish to docker hub as `stackexchange/opserver` or `opserver/opserver`
- Container image just has opserver application in as lightweight a runtime image as we can get away with.
- Can be combined with other images as needed for any application or deployment.
- `opserver:latest` tag is automatically built from master.
- `opserver:x.x.x` tags build from tagged opserver releases.

### Orchestration

- For integration testing, local dev, or sample deployment: can have one or more `docker-compose.yml` that orchestrate opserver with
various dependency combinations. Run redis/sql/elastic/haproxy/opserver containers together as needed, and configure them all to connect appropriately.
- For kubernetes, have a helm chart in repo. Can template out any configuration via user-supplied values. Possibly something that can be published to some package manager or something, but can also just live in repo.


### Config

- Basic container should be usable for "simple" configurations without needing to overwrite config files.
- A sensible default config should be in container, with specifics (single redis instance, sql, elastic maybe) settable with environment variables.
- Secrets (or any setting potentially) should be passable as environment variables. If opserver can't do that natively, can just use a startup script that first runs `envsubst` on the input config file(s) before starting opserver.
- For more complicated configs, you can mount your own config file as volume to the container at runtime as appropriate for your orchestration.
  - `docker run -v /local/path/config.json:/container/config/dir/config.json`
  - docker compose mounts in its own way
  - kubernetes mounts into a deployment via a config map (which helm chart will buid from end-user yaml values).
- Config file may reference environment variables that will get loaded as appropriate (docker run command or kubernetes secrets perhaps), and injected into config files at startup time.

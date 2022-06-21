require "govuk_tech_docs"
require_relative "lib/extensions/govuk_tech_docs/api_reference/api_reference_renderer.rb"
require 'fileutils'

system 'jq --version'
raise "Cannot run `jq` - is it successfully installed?" if $?.exitstatus != 0

# The Qualified Teachers API OpenAPI spec includes a media type that the tech_docs_gem doesn't like:
#
#   application/*+json
#
# Here we're using the `jq` utility to snip it out of the JSON before merging the
# v1 and v2 specs into one, using the `openapi-merge-cli`

FileUtils.mkdir_p 'tmp'
jq_cmd = %s{jq 'walk(if type == "object" and has("application/*+json") then del(."application/*+json") else . end)'}

system "cat ./api-specs/v1.json | #{jq_cmd} > tmp/v1.json"
system "cat ./api-specs/v2.json | #{jq_cmd} > tmp/v2.json"

system 'npx openapi-merge-cli'
raise "Cannot run `npx openapi-merge-cli`. Check that Node is installed and matches the version in .nvmrc" if $?.exitstatus != 0

GovukTechDocs.configure(self)

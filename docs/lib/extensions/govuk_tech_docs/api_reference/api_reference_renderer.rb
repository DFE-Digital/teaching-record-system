require 'govuk_tech_docs/api_reference/api_reference_renderer'

module Extensions
  module GovukTechDocs
    module ApiReference
      module Renderer
        def operations(path, path_id)
          get_operations(path).inject("") do |memo, (key, operation)|
            id = "#{path_id}-#{key.parameterize}"
            parameters = parameters(operation, id)
            request_body = request_body(operation, id)
            responses = responses(operation, id)
            memo + @template_operation.result(binding)
          end
        end

        def get_renderer(file)
          if ["schema.html.erb", "request_body.html.erb", "operation.html.erb"].include?(file)
            template_path = File.join(File.dirname(__FILE__), "templates/" + file)
            template = File.open(template_path, "r").read
            ERB.new(template)
          else
            super(file)
          end
        end

        def request_body(operation, operation_id)
          request_body = operation.request_body
          id = "#{operation_id}-request-body"

          @template_request_body ||= get_renderer("request_body.html.erb")
          @template_request_body.result(binding)
        end
      end
    end
  end
end

class GovukTechDocs::ApiReference::Renderer
  prepend Extensions::GovukTechDocs::ApiReference::Renderer
end

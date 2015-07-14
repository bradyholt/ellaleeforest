require 'open-uri'

class GcontentController < ApplicationController
  before_filter :authenticate, :only => [:edit]

  def initialize(file_id)
    @file_id = file_id
	 super()
  end

  def index
    uri = URI('https://docs.google.com/feeds/download/documents/export/Export?id=' + @file_id + '&exportFormat=html')
  	stream = open(uri)
    content = stream.read
    stream.close
  	parsed_content = Nokogiri::HTML(content)
  	body = parsed_content.at_css("body").inner_html
  	style = parsed_content.at_css("style").to_html
  	@docContent = (style + body)
  end

  def edit
	  @editUrl = "https://docs.google.com/document/d/" + @file_id + "/edit?hl=en_US"
  	expire_action :action => :index
	  redirect_to @editUrl
  end 
end
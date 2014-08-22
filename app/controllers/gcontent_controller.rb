class GcontentController < ApplicationController
  before_filter :authenticate, :only => [:edit]

  def initialize(documentTitle)
	 @docTitle= documentTitle
	 super()
  end

  def index
  	session = GoogleDrive.login(ElfWeb::Application.config.gdata_username, ElfWeb::Application.config.gdata_password)
  	file = session.file_by_title(@docTitle)
  	content = file.download_to_string
  	parsed_content = Nokogiri::HTML(content)
  	body = parsed_content.at_css("body").inner_html
  	style = parsed_content.at_css("style").to_html
  	@docContent = (style + body)
  end

  def edit
  	session = GoogleDrive.login(ElfWeb::Application.config.gdata_username, ElfWeb::Application.config.gdata_password)
	  docId = session.file_by_title(@docTitle).resource_id.sub! 'document:', ''
	  @editUrl = "https://docs.google.com/document/d/" + docId + "/edit?hl=en_US"
	
  	expire_action :action => :index
	  redirect_to @editUrl
  end
end
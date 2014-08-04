class GcontentController < ApplicationController
  before_filter :authenticate, :only => [:edit]

  def initialize(docID, isSpreadsheet=false)
	@docID = docID
	@isSpreadsheet = isSpreadsheet
	super()
  end

  def index
	if @isSpreadsheet == true
		@docContent = Gdata.retrieveSpreadSheet(@docID, Gdata::FORMAT_HTML_BODY_ONLY)
	 else
		@docContent = Gdata.retrieveDocument(@docID, Gdata::FORMAT_HTML_BODY_ONLY)
	 end
  end

  def edit
	if @isSpreadsheet==true
		@editUrl = "https://docs.google.com/spreadsheet/ccc?key=" + @docID + "&hl=en_US"
	else
		@editUrl = "https://docs.google.com/document/d/" + @docID + "/edit?hl=en_US"
	end
 
	expire_action :action => :index
	redirect_to @editUrl
  end
  
  def authenticate
		logger.info "Authenticate user"
		unless logged_in?
		   redirect_to new_authentication_path(:originalUrl => request.fullpath)
		end
  end
end
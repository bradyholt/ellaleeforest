class ReferralsController < ApplicationController
  before_filter :authenticate, :only => [:edit]
  caches_action :index, :layout => false
 
  def index
  		username = ElfWeb::Application.config.gdata_username
  		password = ElfWeb::Application.config.gdata_password
  		key = "1Gm8DnaUyDXaD7oQ_NodOBIawHZrRfPWjgEND6CsUJLk"

		session = GoogleDrive.login(username, password)
		ws = session.spreadsheet_by_key(key).worksheets[0]
		@referrals = []
		for row in 2..ws.num_rows
			@referrals << { :service => ws[row,1], :company => ws[row,2], :contact => ws[row,3], :phone => ws[row,4], :referred_date => ws[row,5], :referred_by => ws[row,6] }
		end
  end

  def edit
	@editUrl = "https://docs.google.com/spreadsheets/d/1Gm8DnaUyDXaD7oQ_NodOBIawHZrRfPWjgEND6CsUJLk/edit?usp=sharing"
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

class ReferralsController < ApplicationController
  before_filter :authenticate, :only => [:edit]
  caches_action :index, :layout => false
 
  def index
  		key = "1Gm8DnaUyDXaD7oQ_NodOBIawHZrRfPWjgEND6CsUJLk"

		client = Google::APIClient.new
		auth = client.authorization
		auth.client_id = ENV["GOOGLE_API_CLIENT_ID"]
		auth.client_secret = ENV["GOOGLE_API_CLIENT_SECRET"]
		auth.scope = [
		  "https://www.googleapis.com/auth/drive",
		  "https://spreadsheets.google.com/feeds/"
		]
		auth.refresh_token = ENV["GOOGLE_API_REFRESH_TOKEN"]
		auth.fetch_access_token!

		session = GoogleDrive.login_with_oauth(auth.access_token)
		ws = session.spreadsheet_by_key(key).worksheets[0]
		@referrals = []
		for row in 2..ws.num_rows
			@referrals << { :service => ws[row,1], :company => ws[row,2], :contact => ws[row,3], :phone => ws[row,4], :referred_date => ws[row,5], :referred_by => ws[row,6] }
		end
  end

  def edit
	@editUrl = "https://docs.google.com/spreadsheets/d/" + key + "/edit?usp=sharing"
	expire_action :action => :index
	redirect_to @editUrl
  end
end

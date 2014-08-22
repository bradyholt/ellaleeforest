class CitizenPatrolsController < ApplicationController
	before_filter :authenticate
	caches_action :index, :layout => false

	def report
		username = ElfWeb::Application.config.gdata_username
  		password = ElfWeb::Application.config.gdata_password
  		reportkey = "0AumP8MJ6pPMwdG03WUQ0SEV6cm5tTklqRF95aWUtNVE"

		@patrol_hours_total = 0
		@hours = Hash.new
		
		column_num = (params[:column] || 2).to_i
		session = GoogleDrive.login(username, password)
		ws = session.spreadsheet_by_key(reportkey).worksheets[0]
		
		for row in 1..ws.num_rows
			col1 = ws[row, 1]
			value = ws[row, column_num]

			if col1.start_with?("Name") 
				@month_year = value
			elsif col1.start_with?("Incidents Reported")
				@incidents_reported = (value == '' ? "0" : value)
			elsif col1.start_with?("Suspects Arrested") 
				@suspects_arrested = (value == '' ? "0" : value)
			elsif col1.start_with?("Report Comments") 
				@comments = value
			elsif col1.start_with?("--")
				next
			elsif col1 == ''
				break
			else
				value = (value == '' ? "0" : value)
				@hours[col1] = value
				@patrol_hours_total += value.to_f
			end
		end

		@patrol_hours_period_1 = (@patrol_hours_total / 4).round(1)
		@patrol_hours_period_2 = @patrol_hours_period_1
		@patrol_hours_period_3 = @patrol_hours_period_1
		@patrol_hours_period_4 = @patrol_hours_total - (@patrol_hours_period_1 * 3)

		render 'report', :layout => 'report'
	end
end

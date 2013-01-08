class ReportController < ApplicationController
	def show
		username = CitizenPatrolReport::Application.config.gdata_username
  		password = CitizenPatrolReport::Application.config.gdata_password
  		reportkey = CitizenPatrolReport::Application.config.gdata_reportkey

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
				@patrol_hours_total += value.to_d
			end
		end

		@patrol_hours_period_1 = (@patrol_hours_total / 4).round(1)
		@patrol_hours_period_2 = @patrol_hours_period_1
		@patrol_hours_period_3 = @patrol_hours_period_1
		@patrol_hours_period_4 = @patrol_hours_total - (@patrol_hours_period_1 * 3)
	end
end

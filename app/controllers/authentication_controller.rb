class AuthenticationController < ApplicationController
	def index		
	end
  
	def authenticate
		authenticated = false
		ElfWeb::Application.config.restricted_area_passwords.each { |x|
			if params[:password].casecmp(x) == 0
				authenticated = true
				break
			end
		}

		if authenticated == true
			cookies[:elfauth] = { :value => 'true', :expires => 6.months.from_now }
			if params[:originalUrl] && !params[:originalUrl].empty?
				redirect_to params[:originalUrl]
			else
				redirect_to '/'
			end
		else
			flash[:error] = "Login attempt was unsuccessful.  Please try again."
			redirect_to :action => :index
		end
	end
	
	def logout
		 cookies.delete(:elfauth)
		 redirect_to "/"
	end
end
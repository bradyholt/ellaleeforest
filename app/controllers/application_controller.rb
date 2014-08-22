class ApplicationController < ActionController::Base
  protect_from_forgery
  helper_method :logged_in?
   
  def logged_in?
		return cookies[:elfauth] && !cookies[:elfauth].empty?
  end	

  def authenticate
		logger.info "Authenticate user"
		unless logged_in?
		   redirect_to new_authentication_path(:originalUrl => request.fullpath)
		end
  end
end

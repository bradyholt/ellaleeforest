
class ApplicationController < ActionController::Base
  protect_from_forgery
  helper_method :logged_in?
   
  def logged_in?
		return cookies[:elfauth] && !cookies[:elfauth].empty?
  end	
end

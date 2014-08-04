class CalendarsController < ApplicationController
  caches_action :index, :layout => false

  def index
  end
end

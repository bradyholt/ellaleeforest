class CalendarsController < ApplicationController
  caches_action :show, :layout => false

  def index
  end
end

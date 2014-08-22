class HoasController < GcontentController
  caches_action :index, :layout => false

  def initialize()
	 super("Home Owners Association")
  end

end

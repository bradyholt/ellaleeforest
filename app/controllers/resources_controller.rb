class ResourcesController < GcontentController
  caches_action :index, :layout => false

  def initialize()
	 super("Resources")
  end
end

class WelcomesController < GcontentController
  caches_action :index, :layout => false
   
  def initialize
	 super("Welcome")
  end
end

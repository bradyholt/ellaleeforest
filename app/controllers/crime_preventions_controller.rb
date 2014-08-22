class CrimePreventionsController < GcontentController
	caches_action :index, :layout => false

	def initialize()
	 super("Crime Prevention")
	end
end

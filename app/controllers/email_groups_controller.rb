class EmailGroupsController < GcontentController
	caches_action :index, :layout => false

	def initialize()
	 super("Email Group")
	end
end

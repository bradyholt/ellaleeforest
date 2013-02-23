class CitizenPatrolsController < GcontentController
	before_filter :authenticate
	caches_action :show, :layout => false

	def initialize()
	 super("18NCEjjb79n19QrkEpT9XLBTVbZFsLdhXJ9tL-VwdB_A")
	end
end

help:
	@echo 'Targets:'
	@echo '    commit.txt'
	@echo '    commit'
	@echo '    docs'
	@echo '    push'

commit.txt:
	hg diff > commit.txt

commit:
	hg commit --logfile commit.txt && rm -f commit.txt

docs:
	doxygen Doxyfile

push:
	hg push 'https://bitbucket.org/flberger/CodeWatchdog'

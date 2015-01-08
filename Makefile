commit.txt:
	hg diff > commit.txt

commit:
	hg commit --logfile commit.txt && rm -f commit.txt

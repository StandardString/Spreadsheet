----------------------------------------------------------------
|			     README.txt                        |
----------------------------------------------------------------

Important git commands (run from inside "[folder]/Spreadsheet/"

1. Updating from repository:

   $ git pull   // Runs "git fetch" and "git merge" to get
                // updated files and merges them with the branch
                // you are currently working in.

2. Committing changes and pushing them to the repository:

   $ git add [item name] // Tracks the item for committing.

   $ git commit -m 'Commit message' // Commits tracked items and
                                    // prepares them for pushing.

   $ git pull   // Updates your copy of the directory with a
                // merge (necessary for push).

   $ git push [remote name ('origin')] [branch name]
     	      	// Pushes your committed items to the remote
		// repository under the specified branch (leave
		// blank to push to the master branch).

3. Checking status of items in working branch:

   $ git status -s  // Lists items as untracked (??), modified
                    // (-M), removed (-R), or tracked (-A).

          or

   $ git status // Provides a more detailed item status report.

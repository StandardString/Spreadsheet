----------------------------------------------------------------
|			  README.txt                           |
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

   $ git status -s  // Provides item status in shorthand.
     	 	    // M - Modified      ?? - Untracked
		    // D - Deleted       A  - Tracked

          or

   $ git status // Provides a more detailed item status report.

4. Checking commit logs:

   $ git log      // Displays commit messages, author, timestamp.
   ctrl+z         // To suspend log command.

5. Creating and switching between branches:

   $ git branch   // Displays branches in working directory.

   $ git branch [name]  // Creates branch of specified name.

   $ git checkout [name] // Switches to the named branch.

   $ git merge [name]  // Merges the branch with the master 
     	       	       // branch.

6. Additional References:

   gitref.org/

----------------------------------------------------------------

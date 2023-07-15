# Release Summary GitHub Action With PR

Customized Step Summary report for workflow run. It will print the summary in following markdown format.

### Output

# release summary
|Version|Time|Author|Commit Id|Commit Message|PR|
|---|---|---|---|---|---|
|[release1.1](https://github.com/kamil467/devops-playground/releases/tag/release1.1)|2023-07-15T05:14:41|@mohamed|b4438026f8fdd44d9da7e067606d1690bc1e26e0|version changed to 2 (#9)  adding more commit message| - [9](https://github.com/kamil467/devops-playground/pull/9) <br />  - Title: version changed to 2 <br /> - Message: <p> </p> | 
  

### How to use this in Workflow Action.
```
                 - name: Release Summary    
                   id: dotnet-code-metrics         
                   uses: kamil467/release-summary-github-action@main     
                   env:   
                      GITHUB_REPO_READ_TOKEN: ${{ secrets.GITHUB_TOKEN }}
                   with:
                      release: 'release1.1'
                 - name: Print Summary
                   run: |
                      cat release-summary.md >> $GITHUB_STEP_SUMMARY
```

#### Parameters:
  -  `GITHUB_REPO_READ_TOKEN` - since it will interact with authenticated github API, you need to pass GH_TOKEN with below permissions.
      
                         
                           permissions:
                                 contents: read
                                 pull-requests: read
                         
  -  release - It should be the name of the release, please pass `default` if you do not have release. since it is a required parameter , you will have to pass some value.

#### What it prints:
   - release name with url
   - time when workflow triggered.
   - actor - who triggered
   - commit id - github sha associated with the trigger
   - commit message
   - PR - Pull request details associated with commit id, This will return only one PR with closed and merged status of assoacited commit.
     
`markdown will be written to release-summary.md file and placed in github/workspace directory.`

It also sets markdown content in `GITHUB_STEP_SUMMARY`

    

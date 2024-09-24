# Git Branch Workflow Guide

This guide outlines how to fetch, switch between branches, and merge code from one branch to another.

## Steps to Get Code from Another Branch

### 1. Fetch the Latest Changes
Before working with branches, ensure you have the latest code from the remote repository:
```bash
git fetch origin
```
### 2. Switch to the Desired Branch
If you want to check or update the code on the desired branch (e.g., `Hoang`), switch to it:

```bash
git checkout Hoang
```

### 3. Pull the Latest Changes on the Branch

Ensure that the Hoang branch is up-to-date by pulling the latest changes:

```bash
git pull origin Hoang
```
### 4. Switch Back to Your Branch
After pulling the latest changes from the desired branch (e.g., `Hoang`), switch back to your working branch (e.g., `Luan`):
```bash
git checkout Luan
```

### 5. Merge the Desired Branch into Your Branch
To merge the changes from the Hoang branch into your current Luan branch, run the following command:

```bash
git merge Hoang
```

### 6. Resolve Any Merge Conflicts
If there are any merge conflicts, Git will notify you. You need to resolve these conflicts manually by editing the files. Once resolved, mark them as resolved:

```bash
git add <file>
```

After resolving and adding the files, commit the merge:
```bash
git commit
```

### 7. Push the Changes to the Remote Repository
Once the merge is complete, push your changes to the remote repository:
```bash
git commit
```

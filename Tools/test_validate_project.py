"""Run with python Tools/test_validate_project.py after staging validator changes."""
import os
from pathlib import Path
import subprocess
import sys
from tempfile import TemporaryDirectory

root = Path(__file__).resolve().parent.parent
scratch = root / "Artifacts"
scratch.mkdir(exist_ok=True)
env = dict(os.environ, GIT_LFS_SKIP_SMUDGE="1")

with TemporaryDirectory(prefix="lfs-validator-", dir=scratch) as folder:
    sandbox = Path(folder).resolve()
    assert sandbox.is_relative_to(scratch.resolve())
    subprocess.run(["git", "checkout-index", "--all", "--prefix=" + sandbox.as_posix() + "/"],
                   cwd=root, env=env, check=True)

    def git(*args):
        subprocess.run(["git", "-c", "core.autocrlf=false", *args], cwd=sandbox,
                       env=env, check=True, capture_output=True)

    def validate():
        return subprocess.run([sys.executable, "Tools/validate_project.py"],
                              cwd=sandbox, capture_output=True, text=True)

    git("init", "--quiet")
    probe = sandbox / "Tools/oversize-probe.bin"
    with probe.open("wb") as stream:
        stream.truncate(50 * 1024 * 1024)
    git("add", "--all")
    result = validate()
    assert result.returncode != 0 and "oversize-probe.bin" in result.stdout + result.stderr, result.stdout + result.stderr

    git("lfs", "track", "Tools/oversize-probe.bin")
    git("add", ".gitattributes", "Tools/oversize-probe.bin")
    result = validate()
    assert result.returncode == 0, result.stdout + result.stderr
    print("PASS: oversized raw blob rejected; hydrated LFS asset and pointer checkout accepted.")

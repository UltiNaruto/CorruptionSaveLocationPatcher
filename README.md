# Corruption Save Location Patcher

This patcher changes the file name of the save to `<Seed Hash>_save.bin` instead of `save.bin`

# Usage

`.\CorruptionSaveLocationPatch <extracted iso path> <seed hash>`

Where `extracted iso path` is the path to the data partition<br><br>
ex :<br>
DATA <--- this is the extracted iso folder<br>
&nbsp;&nbsp;&nbsp;&nbsp;disc<br>
&nbsp;&nbsp;&nbsp;&nbsp;files<br>
&nbsp;&nbsp;&nbsp;&nbsp;sys<br>
&nbsp;&nbsp;&nbsp;&nbsp;cert.bin<br>
&nbsp;&nbsp;&nbsp;&nbsp;h3.bin<br>
&nbsp;&nbsp;&nbsp;&nbsp;ticket.bin<br>
&nbsp;&nbsp;&nbsp;&nbsp;tmd.bin<br>
<br>
and `seed hash` is the 3-4 words hash in the seed details
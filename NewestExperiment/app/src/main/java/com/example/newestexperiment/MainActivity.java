package com.example.newestexperiment;

import androidx.appcompat.app.AppCompatActivity;
import androidx.core.app.ActivityCompat;
import androidx.core.content.ContextCompat;

import android.Manifest;
import android.app.Activity;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.os.Build;
import android.os.Bundle;
import android.os.Environment;
import android.provider.Settings;
import android.util.Log;
import android.view.View;

import com.opencsv.CSVWriter;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.nio.charset.Charset;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

public class MainActivity extends AppCompatActivity {
    static List<Data> defaultData;
    public static List<Integer> userAns;
  //  public static List<Integer> userAns;
    public static long totalTime;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        askForPermissions();
        verifyStoragePermissions(this);
        defaultData =  new ArrayList<>();

        //userAnsB = new ArrayList<>();
        readData();
    }

    public void ExperimentA(View view) {
        userAns = new ArrayList<>();
        Intent intent = new Intent(this, ExperimentA.class);
        startActivity(intent);
    }

    public void ExperimentB(View view) {
        userAns = new ArrayList<>();
        Intent intent = new Intent(this, ExperimentB.class);
        startActivity(intent);
    }
    private static final int REQUEST_EXTERNAL_STORAGE = 1;
    private static String[] PERMISSIONS_STORAGE = {
            Manifest.permission.READ_EXTERNAL_STORAGE,
            Manifest.permission.WRITE_EXTERNAL_STORAGE
    };

    /**
     * Checks if the app has permission to write to device storage
     *
     * If the app does not has permission then the user will be prompted to grant permissions
     *
     * @param activity
     */
    public static void verifyStoragePermissions(Activity activity) {
        // Check if we have write permission
        int permission = ActivityCompat.checkSelfPermission(activity, Manifest.permission.WRITE_EXTERNAL_STORAGE);
        if (ContextCompat.checkSelfPermission(activity, Manifest.permission.CAMERA)
                == PackageManager.PERMISSION_DENIED) {
            ActivityCompat.requestPermissions(activity, new String[] {Manifest.permission.CAMERA}, 100);

        }
        if (permission != PackageManager.PERMISSION_GRANTED) {
            // We don't have permission so prompt the user
            ActivityCompat.requestPermissions(
                    activity,
                    PERMISSIONS_STORAGE,
                    REQUEST_EXTERNAL_STORAGE
            );
        }

        permission = ActivityCompat.checkSelfPermission(activity, Manifest.permission.READ_EXTERNAL_STORAGE);
        if (permission != PackageManager.PERMISSION_GRANTED) {
            // We don't have permission so prompt the user
            Log.i("no","nnn");
            ActivityCompat.requestPermissions(
                    activity,
                    PERMISSIONS_STORAGE,
                    0
            );
        }
    }
    public void askForPermissions() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R) {
            if (!Environment.isExternalStorageManager()) {
                Intent intent = new Intent(Settings.ACTION_MANAGE_ALL_FILES_ACCESS_PERMISSION);
                startActivity(intent);
                return;
            }
        }
    }

    private void readData() {
        InputStream is = getResources().openRawResource(R.raw.newdata2);
        BufferedReader reader = new BufferedReader(
                new InputStreamReader(is, Charset.forName("UTF-8")));
        String line = "";
        try {
            while ((line = reader.readLine()) != null) {
                // Split the line into different tokens (using the comma as a separator).
                line = line.replaceAll("\\uFEFF", "");
                String[] tokens = line.split(",");
                Log.i("tokens", Arrays.toString(tokens));
                Data data = new Data(Integer.valueOf(tokens[0]) - 1, Arrays.copyOfRange(tokens, 1, 6),
                        Integer.valueOf(tokens[6]), Integer.valueOf(tokens[7]),
                        Integer.valueOf(tokens[8]), Integer.valueOf(tokens[9]));
                defaultData.add(data);

                Log.d("MainActivity" ,"Just Created " + data.toString());
            }
        } catch (IOException e1) {
            Log.e("MainActivity", "Error" + line, e1);
            e1.printStackTrace();
        }
    }


    public static void writeData(String[] data) throws IOException {
        String baseDir = android.os.Environment.getExternalStorageDirectory().getAbsolutePath();
        String fileName = "AnalysisData.csv";
        String filePath = baseDir + "/experiment" + File.separator + fileName;
        File f = new File(filePath);
        f.createNewFile();
        Log.i("hhhh", f.getPath());
        CSVWriter writer;


        // File exist
        FileWriter mFileWriter;
        if(f.exists()&&!f.isDirectory())
        {
            mFileWriter = new FileWriter(filePath, true);
            writer = new CSVWriter(mFileWriter);
        }
        else
        {
            writer = new CSVWriter(new FileWriter(filePath));
        }

        //String[] data = {"Ship Name", "Scientist Name", "...", new SimpleDateFormat("yyyy-MM-dd HH:mm:ss").formatter.format(date)});
        //String[] data = {"aa", "bb", "cc"};
        writer.writeNext(data);

        writer.close();
    }
}